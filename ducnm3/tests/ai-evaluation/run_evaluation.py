#!/usr/bin/env python3
"""Chạy đánh giá chức năng AI (kịch bản AI-01 … AI-12) trên hệ thống thật qua API Gateway.

Nguyên tắc:
  * Ghi kết quả NGAY sau mỗi bước vào results/state.json (và results/raw/*) -> bị ngắt giữa chừng thì chạy lại
    cùng lệnh, các bước đã xong được bỏ qua, bước dang dở được tiếp tục (job đã tạo vẫn được poll tiếp).
  * Gặp lỗi hết hạn mức / token / xác thực nhà cung cấp (AI_PROVIDER_RATE_LIMITED, AI_PROVIDER_AUTH_FAILED,
    HTTP 429) NGOÀI kịch bản cố ý gây lỗi -> DỪNG NGAY, ghi results/STOPPED.json, thoát mã 3.
  * Cấu hình AI bị đổi tạm (AI-10, AI-12) luôn được khôi phục; nếu bị ngắt giữa chừng thì lần chạy sau khôi phục
    trước khi làm việc khác (state['pending_restore']).

Chạy:   python3 run_evaluation.py            # chạy tất cả bước chưa xong
        python3 run_evaluation.py --status   # xem tiến độ
        python3 run_evaluation.py --only AI-01,AI-02
Yêu cầu: hệ thống đang chạy (docker compose up), python3 (chỉ thư viện chuẩn), docker để đọc DB.
"""
import argparse
import json
import mimetypes
import os
import subprocess
import sys
import time
import urllib.error
import urllib.request
import uuid
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parent
DATASET = ROOT / "dataset"
RESULTS = ROOT / "results"
RAW = RESULTS / "raw"
STATE_FILE = RESULTS / "state.json"

GATEWAY = os.environ.get("LMS_GATEWAY", "http://localhost:5100")
ADMIN_ID = "00000000-0000-0000-0000-000000000001"
MYSQL_CONTAINER = os.environ.get("LMS_MYSQL_CONTAINER", "ducnm3-mysql-1")
STOP_CODES = {"AI_PROVIDER_RATE_LIMITED", "AI_PROVIDER_AUTH_FAILED"}
POLL_TIMEOUT = 600


class StopEvaluation(Exception):
    pass


# ----------------------------------------------------------------------------- state
def now():
    return datetime.now(timezone.utc).isoformat(timespec="seconds")


def load_state():
    if STATE_FILE.exists():
        return json.loads(STATE_FILE.read_text(encoding="utf-8"))
    return {"started_at": now(), "steps": {}, "ids": {}, "pending_restore": None, "intentional": False}


STATE = None


def save():
    RESULTS.mkdir(exist_ok=True)
    RAW.mkdir(exist_ok=True)
    tmp = STATE_FILE.with_suffix(".tmp")
    tmp.write_text(json.dumps(STATE, ensure_ascii=False, indent=1), encoding="utf-8")
    tmp.replace(STATE_FILE)


def raw(name, data):
    RAW.mkdir(parents=True, exist_ok=True)
    p = RAW / name
    if isinstance(data, (dict, list)):
        p.write_text(json.dumps(data, ensure_ascii=False, indent=1), encoding="utf-8")
    else:
        p.write_text(str(data), encoding="utf-8")


def stop(reason):
    RESULTS.mkdir(exist_ok=True)
    (RESULTS / "STOPPED.json").write_text(json.dumps({"at": now(), "reason": reason}, ensure_ascii=False, indent=1), encoding="utf-8")
    save()
    raise StopEvaluation(reason)


# ----------------------------------------------------------------------------- http
def http(method, path, body=None, headers=None, raw_body=None, timeout=120):
    url = GATEWAY + path
    h = {"X-Actor-Type": "ADMIN", "X-Actor-Id": ADMIN_ID}
    if headers:
        h.update(headers)
    data = raw_body
    if body is not None:
        data = json.dumps(body).encode()
        h["Content-Type"] = "application/json"
    req = urllib.request.Request(url, data=data, method=method, headers=h)
    try:
        with urllib.request.urlopen(req, timeout=timeout) as r:
            txt = r.read().decode("utf-8", "replace")
            return r.status, (json.loads(txt) if txt.strip() else None)
    except urllib.error.HTTPError as e:
        txt = e.read().decode("utf-8", "replace")
        try:
            js = json.loads(txt)
        except Exception:
            js = {"raw": txt}
        if e.code == 429 and not STATE.get("intentional"):
            stop(f"HTTP 429 từ {method} {path}")
        return e.code, js


def multipart(path, fields, file_field, file_path):
    boundary = "----eval" + uuid.uuid4().hex
    ctype = mimetypes.guess_type(file_path.name)[0] or "application/octet-stream"
    if file_path.suffix == ".md":
        ctype = "text/markdown"
    parts = []
    for k, v in fields.items():
        parts.append(f'--{boundary}\r\nContent-Disposition: form-data; name="{k}"\r\n\r\n{v}\r\n'.encode())
    parts.append((f'--{boundary}\r\nContent-Disposition: form-data; name="{file_field}"; filename="{file_path.name}"\r\n'
                  f'Content-Type: {ctype}\r\n\r\n').encode() + file_path.read_bytes() + b"\r\n")
    parts.append(f"--{boundary}--\r\n".encode())
    return http("POST", path, raw_body=b"".join(parts),
                headers={"Content-Type": f"multipart/form-data; boundary={boundary}"}, timeout=300)


# ----------------------------------------------------------------------------- db
def sql(db, query):
    cmd = ["docker", "exec", "-i", MYSQL_CONTAINER, "sh", "-c",
           f'mysql -uroot -p"$MYSQL_ROOT_PASSWORD" --default-character-set=utf8mb4 -B --raw {db} 2>/dev/null']
    out = subprocess.run(cmd, input=query.encode(), capture_output=True, check=True).stdout.decode("utf-8", "replace")
    lines = out.strip("\n").split("\n")
    if not lines or not lines[0]:
        return []
    head = lines[0].split("\t")
    return [dict(zip(head, [None if c == "NULL" else c for c in ln.split("\t")])) for ln in lines[1:]]


def check_invocations():
    """Dừng ngay nếu có lần gọi AI mới thất bại do hết hạn mức/xác thực ngoài kịch bản cố ý."""
    since = STATE["started_at_db"]
    rows = sql("lms_ai_db", f"SELECT id, task_name, model_name, status, error_code, created_at FROM ai_invocations "
                            f"WHERE created_at >= '{since}' AND error_code IS NOT NULL AND error_code <> 'NULL' ORDER BY created_at")
    seen = set(STATE.setdefault("seen_failures", []))
    for r in rows:
        if r["id"] in seen:
            continue
        seen.add(r["id"])
        STATE["seen_failures"] = sorted(seen)
        save()
        if r["error_code"] in STOP_CODES and not STATE.get("intentional"):
            stop(f"ai_invocations: {r['error_code']} (task {r['task_name']}, model {r['model_name']})")


# ----------------------------------------------------------------------------- steps framework
def step(name, fn):
    s = STATE["steps"].get(name)
    if s and s.get("status") == "done":
        print(f"  [skip] {name}")
        return s["result"]
    print(f"  [run ] {name}")
    t0 = time.time()
    res = fn()
    STATE["steps"][name] = {"status": "done", "finished_at": now(), "seconds": round(time.time() - t0, 1), "result": res}
    save()
    check_invocations()
    return res


def need(code, ok, what, resp):
    if code not in ok:
        raise RuntimeError(f"{what}: HTTP {code} {json.dumps(resp, ensure_ascii=False)[:400]}")


# ----------------------------------------------------------------------------- domain helpers
def ensure_course():
    if "course" in STATE["ids"]:
        return STATE["ids"]["course"]
    code, js = http("POST", "/course/api/courses", {"name": f"AI-EVAL {STATE['started_at'][:16]}", "descriptionMarkdown": "Khóa học tạm để đánh giá chức năng AI"})
    need(code, (201,), "tạo course", js)
    STATE["ids"]["course"] = js["data"]["id"]
    save()
    return STATE["ids"]["course"]


def ensure_lesson(key, order, content=None):
    ids = STATE["ids"].setdefault("lessons", {})
    if key in ids:
        return ids[key]
    c = ensure_course()
    body = {"title": f"Eval {key}", "displayOrder": len(ids) + 1}  # displayOrder phải duy nhất trong course
    if content is not None:
        body["contentMarkdown"] = content
    code, js = http("POST", f"/course/api/courses/{c}/lessons", body)
    need(code, (201,), f"tạo lesson {key}", js)
    ids[key] = js["data"]["id"]
    save()
    return ids[key]


def student_id():
    if "student" not in STATE["ids"]:
        code, js = http("GET", "/student/api/students?pageSize=1")
        need(code, (200,), "lấy student", js)
        STATE["ids"]["student"] = js["data"][0]["id"]
        save()
    return STATE["ids"]["student"]


def upload_attach(lesson_key, ds_file, order):
    """Tải tệp lên Media Service rồi gắn làm tệp đính kèm của lesson. Trả mediaId."""
    media = STATE["ids"].setdefault("media", {})
    mk = f"{lesson_key}|{ds_file}"
    if mk in media:
        return media[mk]
    lesson = ensure_lesson(lesson_key, order)
    code, js = multipart("/media/api/media", {"mediaType": "DOCUMENT", "uploadedByType": "STUDENT", "uploadedBy": student_id()}, "file", DATASET / ds_file)
    need(code, (201,), f"upload {ds_file}", js)
    mid = js["data"]["id"]
    code, js2 = http("POST", "/media/api/media/usages", {"mediaId": mid, "ownerService": "COURSE", "ownerType": "LESSON_ATTACHMENT",
                                                         "ownerId": lesson, "usageType": "ATTACHMENT", "displayOrder": 0})
    need(code, (200, 201), f"gắn {ds_file}", js2)
    media[mk] = mid
    save()
    return mid


def sync_and_wait(lesson_key, media_id, retry=False):
    c, l = ensure_course(), STATE["ids"]["lessons"][lesson_key]
    t0 = time.time()
    code, js = http("POST", f"/course/api/courses/{c}/lessons/{l}/ai-documents/sync" + ("?retryFailed=true" if retry else ""))
    need(code, (200,), "sync", js)
    while time.time() - t0 < POLL_TIMEOUT:
        code, js = http("GET", f"/course/api/courses/{c}/lessons/{l}/ai-documents")
        need(code, (200,), "list docs", js)
        doc = next((i for i in js["data"]["items"] if i["mediaId"] == media_id), None)
        if doc and doc["status"] in ("EXTRACTED", "FAILED", "UNSUPPORTED"):
            doc["elapsedSeconds"] = round(time.time() - t0, 1)
            return doc
        time.sleep(3)
    raise RuntimeError("quá thời gian chờ trích xuất")


def create_job(lesson_key, job_type, media_ids, question_count=None, overwrite=False, tag=""):
    jobs = STATE["ids"].setdefault("jobs", {})
    jk = f"{lesson_key}|{job_type}|{','.join(sorted(media_ids))}|{overwrite}|{tag}"
    if jk in jobs:
        return jobs[jk]
    c, l = ensure_course(), STATE["ids"]["lessons"][lesson_key]
    body = {"jobType": job_type, "mediaIds": media_ids, "overwriteContent": overwrite}
    if question_count:
        body["questionCount"] = question_count
    code, js = http("POST", f"/course/api/courses/{c}/lessons/{l}/ai-jobs", body)
    if code != 202:
        return {"httpStatus": code, "error": js, "jobId": None}
    jobs[jk] = {"httpStatus": 202, "jobId": js["data"]["id"]}
    save()
    return jobs[jk]


def wait_job(lesson_key, job_id):
    c, l = ensure_course(), STATE["ids"]["lessons"][lesson_key]
    t0 = time.time()
    while time.time() - t0 < POLL_TIMEOUT:
        code, js = http("GET", f"/course/api/courses/{c}/lessons/{l}/ai-jobs/{job_id}")
        need(code, (200,), "get job", js)
        job = js["data"]["job"] if "job" in js["data"] else js["data"]
        if job["status"] in ("COMPLETED", "FAILED"):
            job["elapsedSeconds"] = round(time.time() - t0, 1)
            job["previousContentMarkdown"] = js["data"].get("previousContentMarkdown")
            return job
        time.sleep(3)
    raise RuntimeError("quá thời gian chờ job")


def run_job(lesson_key, job_type, media_ids, question_count=None, overwrite=False, tag=""):
    j = create_job(lesson_key, job_type, media_ids, question_count, overwrite, tag)
    if not j.get("jobId"):
        return {"created": False, **j}
    job = wait_job(lesson_key, j["jobId"])
    job["created"] = True
    return job


def extracted_text(lesson_key, media_id):
    """Đọc văn bản đã trích xuất. Dùng TO_BASE64 để không mất xuống dòng / ký tự Unicode khi qua mysql -B."""
    import base64
    l = STATE["ids"]["lessons"][lesson_key]
    rows = sql("lms_course_db", f"SELECT status, error_code, CHAR_LENGTH(extracted_text) AS chars, "
                                f"REPLACE(TO_BASE64(extracted_text), '\\n', '') AS b64 FROM lesson_attachment_texts "
                                f"WHERE lesson_id='{l}' AND media_id='{media_id}'")
    if not rows:
        return None
    r = rows[0]
    r["extracted_text"] = base64.b64decode(r["b64"]).decode("utf-8") if r.get("b64") else None
    return r


def lesson_content(lesson_key):
    c, l = ensure_course(), STATE["ids"]["lessons"][lesson_key]
    code, js = http("GET", f"/course/api/courses/{c}/lessons/{l}")
    need(code, (200,), "get lesson", js)
    d = js["data"]
    return d.get("contentMarkdown") or d.get("content") or ""


def get_quiz(lesson_key, quiz_id):
    c, l = ensure_course(), STATE["ids"]["lessons"][lesson_key]
    code, js = http("GET", f"/course/api/courses/{c}/lessons/{l}/quizzes/{quiz_id}")
    need(code, (200,), "get quiz", js)
    return js["data"]


# ----------------------------------------------------------------------------- restore config
def binding(service):
    code, js = http("GET", "/ai/api/ai/service-bindings")
    need(code, (200,), "bindings", js)
    return next(b for b in js["data"] if b["serviceName"] == service)


def set_binding(service, model_id):
    code, js = http("PUT", f"/ai/api/ai/service-bindings/{service}", {"modelId": model_id})
    need(code, (200, 201), f"đổi binding {service}", js)


def restore_pending():
    p = STATE.get("pending_restore")
    if not p:
        return
    print(f"  [restore] khôi phục binding {p['service']} -> {p['model_id']}")
    set_binding(p["service"], p["model_id"])
    for kind, rid in (("models", p.get("temp_model")), ("providers", p.get("temp_provider"))):
        if rid:
            http("DELETE", f"/ai/api/ai/{kind}/{rid}")
    check_invocations()  # đánh dấu các lỗi cố ý đã thấy, để không bị coi là hết hạn mức
    STATE["pending_restore"] = None
    STATE["intentional"] = False
    save()


# ----------------------------------------------------------------------------- scenarios
def sc_extract(sid, ds_file, order, expect_fail_code=None):
    def fn():
        before = sql("lms_ai_db", f"SELECT COUNT(*) n FROM ai_invocations WHERE task_name='document-extraction'")[0]["n"]
        mid = upload_attach(sid, ds_file, order)
        doc = sync_and_wait(sid, mid)
        after = sql("lms_ai_db", f"SELECT COUNT(*) n FROM ai_invocations WHERE task_name='document-extraction'")[0]["n"]
        out = {"dataset": ds_file, "mediaId": mid, "doc": doc, "aiCalls": int(after) - int(before)}
        t = extracted_text(sid, mid)
        if t and t.get("extracted_text"):
            raw(f"{sid}-{Path(ds_file).stem}.txt", t["extracted_text"])
            out["textChars"] = int(t["chars"])
        return out
    return step(f"{sid}:extract:{ds_file}", fn)


def scenarios():
    S = {}

    def ai01():
        sc_extract("AI-01", "DS-01-pdf-van-ban.pdf", 1)
    S["AI-01"] = ai01

    def ai02():
        sc_extract("AI-02a", "DS-02-pdf-cong-thuc.pdf", 1)
        sc_extract("AI-02b", "DS-03-pdf-quet.pdf", 2)
    S["AI-02"] = ai02

    def ai03():
        sc_extract("AI-03", "DS-06b-pdf-vuot-gioi-han.pdf", 1)
        sc_extract("AI-03a", "DS-06a-pdf-lon-sat-gioi-han.pdf", 1)  # sát giới hạn: phải được chấp nhận
    S["AI-03"] = ai03

    def ai04():
        sc_extract("AI-04", "DS-06c-txt-qua-300k-ky-tu.txt", 1)
    S["AI-04"] = ai04

    def ai05():
        a = sc_extract("AI-05", "DS-01-pdf-van-ban.pdf", 1)
        # gộp hai tài liệu vào cùng một lesson để tổng hợp
        def fn():
            m2 = upload_attach("AI-05", "DS-04-docx.docx", 2)
            doc = sync_and_wait("AI-05", m2)
            job = run_job("AI-05", "SYNTHESIZE_LESSON", [a["mediaId"], m2])
            content = lesson_content("AI-05")
            raw("AI-05-lesson.md", content)
            return {"doc2": doc, "job": job, "contentChars": len(content)}
        step("AI-05:synthesize", fn)
    S["AI-05"] = ai05

    def ai06():
        mid = upload_attach("AI-06", "DS-01-pdf-van-ban.pdf", 1)
        step("AI-06:extract", lambda: sync_and_wait("AI-06", mid))
        def first():
            return run_job("AI-06", "SYNTHESIZE_LESSON", [mid], overwrite=False)
        r1 = step("AI-06:synth-first", first)
        def blocked():
            c, l = ensure_course(), STATE["ids"]["lessons"]["AI-06"]
            code, js = http("POST", f"/course/api/courses/{c}/lessons/{l}/ai-jobs",
                            {"jobType": "SYNTHESIZE_LESSON", "mediaIds": [mid], "overwriteContent": False})
            return {"httpStatus": code, "body": js}
        step("AI-06:overwrite-false", blocked)
        def over():
            job = run_job("AI-06", "SYNTHESIZE_LESSON", [mid], overwrite=True)
            return {"job": job, "contentChars": len(lesson_content("AI-06"))}
        step("AI-06:overwrite-true", over)
    S["AI-06"] = ai06

    def ai07():
        mid = upload_attach("AI-07", "DS-05-markdown.md", 1)
        step("AI-07:extract", lambda: sync_and_wait("AI-07", mid))
        def fn():
            job = run_job("AI-07", "GENERATE_QUIZ", [mid], question_count=10)
            out = {"job": job}
            if job.get("resultQuizId"):
                q = get_quiz("AI-07", job["resultQuizId"])
                raw("AI-07-quiz.json", q)
                out["quizStatus"] = q.get("status")
            return out
        step("AI-07:quiz", fn)
    S["AI-07"] = ai07

    def ai08():
        for key, ds, order in (("AI-08a", "DS-02-pdf-cong-thuc.pdf", 1), ("AI-08b", "DS-05-markdown.md", 1)):
            mid = upload_attach(key, ds, order)
            step(f"{key}:extract", lambda k=key, m=mid: sync_and_wait(k, m))
            def synth(k=key, m=mid):
                job = run_job(k, "SYNTHESIZE_LESSON", [m])
                content = lesson_content(k)
                raw(f"{k}-lesson.md", content)
                return {"job": job, "contentChars": len(content)}
            step(f"{key}:synthesize", synth)
            def quiz(k=key, m=mid):
                job = run_job(k, "GENERATE_QUIZ", [m], question_count=10)
                out = {"job": job}
                if job.get("resultQuizId"):
                    q = get_quiz(k, job["resultQuizId"])
                    raw(f"{k}-quiz.json", q)
                return out
            step(f"{key}:quiz", quiz)
    S["AI-08"] = ai08

    def ai09():
        mid = upload_attach("AI-09", "DS-07-pdf-chi-dan-doc-hai.pdf", 1)
        # lesson "đối chứng" để kiểm tra không bị ghi ngoài phạm vi
        ctrl = ensure_lesson("AI-09-control", 2, content="NỘI DUNG ĐỐI CHỨNG KHÔNG ĐƯỢC THAY ĐỔI")
        step("AI-09:extract", lambda: sync_and_wait("AI-09", mid))
        def synth():
            job = run_job("AI-09", "SYNTHESIZE_LESSON", [mid])
            content = lesson_content("AI-09")
            raw("AI-09-lesson.md", content)
            return {"job": job, "contentChars": len(content), "canaryInLesson": "CANARY-7F3A" in content,
                    "controlUnchanged": lesson_content("AI-09-control") == "NỘI DUNG ĐỐI CHỨNG KHÔNG ĐƯỢC THAY ĐỔI"}
        step("AI-09:synthesize", synth)
        def quiz():
            job = run_job("AI-09", "GENERATE_QUIZ", [mid], question_count=5)
            out = {"job": job}
            if job.get("resultQuizId"):
                q = get_quiz("AI-09", job["resultQuizId"])
                raw("AI-09-quiz.json", q)
                out["canaryInQuiz"] = "CANARY-7F3A" in json.dumps(q, ensure_ascii=False)
            return out
        step("AI-09:quiz", quiz)
    S["AI-09"] = ai09

    def ai10():
        orig = binding("lesson-synthesis")
        mid = upload_attach("AI-10", "DS-01-pdf-van-ban.pdf", 1)
        # cần tài liệu EXTRACTED: dùng lại bản trích xuất sẵn nếu có
        step("AI-10:extract", lambda: sync_and_wait("AI-10", mid))
        for tag, base_url, key in (("auth", None, "sk-invalid-key-for-evaluation-0000"), ("unreachable", "http://127.0.0.1:9", "sk-unreachable-0000")):
            def fn(tag=tag, base_url=base_url, key=key):
                STATE["intentional"] = True
                STATE["pending_restore"] = {"service": "lesson-synthesis", "model_id": orig["modelId"]}
                save()
                suffix = uuid.uuid4().hex[:6]
                ptype = "OPENAI" if tag == "auth" else "CUSTOM"
                code, p = http("POST", "/ai/api/ai/providers", {"name": f"eval-temp-{tag}-{suffix}", "providerType": ptype, "baseUrl": base_url, "apiKey": key})
                need(code, (201,), "tạo provider tạm", p)
                STATE["pending_restore"]["temp_provider"] = p["data"]["id"]
                save()
                code, m = http("POST", "/ai/api/ai/models", {"providerId": p["data"]["id"], "modelName": "gpt-5.4-mini" if tag == "auth" else "dummy-model"})
                need(code, (201,), "tạo model tạm", m)
                STATE["pending_restore"]["temp_model"] = m["data"]["id"]
                save()
                set_binding("lesson-synthesis", m["data"]["id"])
                job = run_job("AI-10", "SYNTHESIZE_LESSON", [STATE["ids"]["media"]["AI-10|DS-01-pdf-van-ban.pdf"]], overwrite=True, tag=tag)
                restore_pending()
                return {"case": tag, "job": job}
            step(f"AI-10:{tag}", fn)
    S["AI-10"] = ai10

    def ai11():
        def fn():
            code, js = http("POST", "/ai/api/ai/completions", {"task": "course-service", "messages": [{"role": "user", "text": "ping"}]})
            return {"httpStatus": code, "body": js}
        step("AI-11:unconfigured-task", fn)
    S["AI-11"] = ai11

    def ai12():
        mid = upload_attach("AI-12", "DS-05-markdown.md", 1)
        step("AI-12:extract", lambda: sync_and_wait("AI-12", mid))
        orig = binding("quiz-authoring")
        alt = binding("lesson-synthesis")  # gpt-5.4-mini
        def fn():
            STATE["pending_restore"] = {"service": "quiz-authoring", "model_id": orig["modelId"]}
            save()
            set_binding("quiz-authoring", alt["modelId"])
            t0 = now()
            job = run_job("AI-12", "GENERATE_QUIZ", [mid], question_count=5)
            models = sql("lms_ai_db", f"SELECT task_name, model_name, status FROM ai_invocations WHERE task_name='quiz-authoring' ORDER BY created_at DESC LIMIT 3")
            restore_pending()
            return {"originalModel": orig["modelName"], "switchedTo": alt["modelName"], "job": job, "lastInvocations": models}
        step("AI-12:switch-model", fn)
    S["AI-12"] = ai12

    def retest():
        """Tái kiểm sau khi sửa BrokenEscapeGuard: soạn lại quiz từ DS-05 ba lần (tài liệu AI-08b)."""
        mid = STATE["ids"]["media"]["AI-08b|DS-05-markdown.md"]
        for n in (1, 2, 3, 4):
            def fn(n=n):
                job = run_job("AI-08b", "GENERATE_QUIZ", [mid], question_count=10, tag=f"retest-{n}")
                out = {"job": job}
                if job.get("resultQuizId"):
                    raw(f"RETEST-08-{n}-quiz.json", get_quiz("AI-08b", job["resultQuizId"]))
                return out
            step(f"RETEST-08:quiz-{n}", fn)
    S["RETEST-08"] = retest
    return S


def metrics():
    """Tổng hợp số liệu vận hành từ DB sau khi chạy (ghi results/metrics.json)."""
    since = STATE["started_at_db"]
    inv = sql("lms_ai_db", f"SELECT task_name, model_name, status, COUNT(*) calls, SUM(prompt_tokens) prompt_tokens, "
                           f"SUM(completion_tokens) completion_tokens, ROUND(AVG(latency_ms)) avg_latency_ms, MAX(latency_ms) max_latency_ms "
                           f"FROM ai_invocations WHERE created_at >= '{since}' GROUP BY task_name, model_name, status ORDER BY task_name, model_name, status")
    lat = sql("lms_ai_db", f"SELECT task_name, latency_ms FROM ai_invocations WHERE created_at >= '{since}' AND status='SUCCESS' ORDER BY task_name, latency_ms")
    p95 = {}
    by = {}
    for r in lat:
        by.setdefault(r["task_name"], []).append(int(r["latency_ms"]))
    for t, v in by.items():
        p95[t] = {"n": len(v), "p50": v[int(0.5 * (len(v) - 1))], "p95": v[min(len(v) - 1, int(round(0.95 * (len(v) - 1))))]}
    c = STATE["ids"].get("course", "")
    jobs = sql("lms_course_db", f"SELECT job_type, status, error_code, COUNT(*) n, ROUND(AVG(step_count),1) avg_steps FROM lesson_ai_jobs "
                                f"WHERE course_id='{c}' GROUP BY job_type, status, error_code")
    docs = sql("lms_course_db", f"SELECT status, error_code, COUNT(*) n FROM lesson_attachment_texts WHERE lesson_id IN "
                                f"(SELECT id FROM lessons WHERE course_id='{c}') GROUP BY status, error_code")
    out = {"since": since, "invocations": inv, "latencySuccess": p95, "jobs": jobs, "documents": docs}
    RESULTS.mkdir(exist_ok=True)
    (RESULTS / "metrics.json").write_text(json.dumps(out, ensure_ascii=False, indent=1), encoding="utf-8")
    return out


def status():
    S = scenarios()
    print(f"bắt đầu: {STATE['started_at']}  | restore chờ: {STATE.get('pending_restore')}")
    for name, s in STATE["steps"].items():
        print(f"  {s['status']:5} {name}  ({s.get('seconds')}s)")
    if (RESULTS / "STOPPED.json").exists():
        print("ĐÃ DỪNG:", (RESULTS / "STOPPED.json").read_text(encoding="utf-8"))


def main():
    global STATE
    ap = argparse.ArgumentParser()
    ap.add_argument("--only")
    ap.add_argument("--status", action="store_true")
    ap.add_argument("--metrics", action="store_true")
    a = ap.parse_args()
    RESULTS.mkdir(exist_ok=True)
    STATE = load_state()
    if "started_at_db" not in STATE:
        STATE["started_at_db"] = sql("lms_ai_db", "SELECT DATE_FORMAT(NOW(6), '%Y-%m-%d %H:%i:%s.%f') t")[0]["t"]
        save()
    if a.status:
        return status()
    if (RESULTS / "STOPPED.json").exists():
        print("Lần chạy trước đã DỪNG vì hết hạn mức; xóa results/STOPPED.json sau khi đã xử lý nguyên nhân rồi chạy lại.")
        print((RESULTS / "STOPPED.json").read_text(encoding="utf-8"))
        return 3
    try:
        restore_pending()
        S = scenarios()
        todo = a.only.split(",") if a.only else list(S)
        for name in todo:
            print(f"== {name}")
            S[name]()
        m = metrics()
        print("Xong. Số liệu tổng hợp: results/metrics.json")
    except StopEvaluation as e:
        print(f"\n!!! DỪNG NGAY: {e}\nTiến độ đã được lưu; xử lý hạn mức rồi xóa results/STOPPED.json và chạy lại để tiếp tục.")
        return 3
    return 0


if __name__ == "__main__":
    sys.exit(main())
