# Quy trình làm task của Developer

Tài liệu nội bộ giúp Developer hiểu quy trình xử lý **một task Jira** theo cấu
trúc WBS — từ Estimate đến Thực thi test — và chuẩn ghi nhận / attach tài liệu
trên cùng ticket đó. Áp dụng từ **Ngày 3**. Ngày 1–2 giữ lịch sử đã làm trên
nhánh `ducnm3`.

| | |
| --- | --- |
| **Đối tượng đọc** | Developer phụ trách task (tham chiếu thêm SQA khi phối hợp) |
| **Mục tiêu** | Nắm phase, hạng mục trên Jira, việc dev phải làm và tài liệu cần gắn vào task |
| **Ví dụ task** | `【BLD-124_4】 Đối ứng gửi mail khi dừng user tích hợp` |

Quy ước plan: mỗi hạng mục trên `docs/plan/` phải có **Task**, **Est** và
**Ticket**. User tạo ticket trên Jira; agent không tạo Jira. Khi user gửi mã,
điền cột Ticket. `Chưa tạo` hoặc trống = **cấm push code**. Markdown tài liệu
(`docs/**/*.md`, `AGENTS.md`, `rules/*.md`, `.agents/**/*.md`) được push thẳng
lên `ducnm3`.

---

## 1. Bạn cần nắm gì

Sau khi đọc, developer phụ trách task cần trả lời được:

1. **Một task Jira** có cấu trúc như thế nào (không tách nhiều sub-task theo
   phase).
2. Các **phase** trên bảng WBS: Estimate → Design → Implement → Thực thi test.
3. **Dòng nào thuộc dev** trên WBS — dev làm gì, output gì, link tài liệu ở đâu
   trên ticket và trong repo.
4. **SQA / R1 / R2** chạy song song hoặc sau dev ở bước nào để phối hợp đúng.
5. Khi nào được coi là **xong** từng hạng mục và **Done** cả task.

---

## 2. Cấu trúc một task Jira

Mỗi hạng mục dev nhận là **một ticket Jira duy nhất**, title dạng:

```text
【BLD-124_4】 Đối ứng gửi mail khi dừng user tích hợp
```

Toàn bộ vòng đời nằm trên **một ticket**. Đây không phải mô hình “mỗi phase một
sub-task Jira riêng”. Dev cập nhật tiến độ và attach/link tài liệu trên ticket
đó.

Trên `docs/plan/` cùng hạng mục có ba cột:

| Cột | Ý nghĩa |
| --- | --- |
| Task | Một hạng mục = một ticket Jira |
| Est | Estimate giờ thật của Dev; không bắt buộc tổng ngày = 8 giờ |
| Ticket | Mã Jira, ví dụ `BLD-124_4`. `Chưa tạo` = cấm push code |

Nhánh git là `feature/{mã backlog}` (`feature/BLD-124_4`), không dùng tiêu đề
đầy đủ `【BLD-124_4】 …` và không dùng `feature/ducnm3_*`.

---

## 3. Map phase WBS sang MiniProject

| Phase WBS | Việc Dev / agent làm | Artifact trên ticket / repo |
| --- | --- | --- |
| Estimate | Đọc spec/plan, điều tra code, điền Est | Cột Est trên `docs/plan/`; link spec trên Jira Description |
| Design | Q&A; PTYC và PVAH | `docs/api/`, `docs/business-flows/`, `docs/architecture/`, `docs/database/`; link trên Jira |
| Implement | Coding trên `feature/{mã backlog}`; self-test; tạo PR khi user yêu cầu | Link PR trên Jira; checklist self-test |
| Thực thi test | Support test; fix bug | Comment Jira: nguyên nhân, phạm vi ảnh hưởng, hướng fix; PR fix |

SQA / R1 / R2 là vai trò phối hợp. Dev không làm thay SQA, nhưng phải biết khi
nào SQA cần PTYC/test case và khi nào vào Verify Bug.

---

## 4. Luồng git bắt buộc từ Ngày 3

1. User tạo ticket Jira. Agent cập nhật cột Ticket trên `docs/plan/` khi nhận
   mã. Agent không tạo Jira.
2. Base nhánh `ducnm3`. Tạo hoặc chuyển sang `feature/{mã backlog}`, ví dụ
   `feature/BLD-124_4`.
3. Commit message viết bằng tiếng Việt, ngắn gọn nhưng nêu rõ hành động và phạm
   vi thay đổi chính. Dùng nội dung task backlog khi tiêu đề task đã rõ nghĩa;
   không dùng `feat:` hoặc message chung chung như `update`, `fix`, `wip`.
4. Push **code** chỉ khi Ticket đã có mã **và** user yêu cầu push. Nếu Ticket là
   `Chưa tạo` hoặc trống thì **không push code**, kể cả khi user bảo push. Commit
   local vẫn được.
5. **Ngoại lệ docs:** thay đổi chỉ gồm markdown tài liệu (`docs/**/*.md`,
   `AGENTS.md`, `rules/*.md`, `.agents/**/*.md`) được commit và push thẳng lên
   `ducnm3`. Không cần ticket, không bắt buộc PR. Diff có code thì không dùng
   ngoại lệ này.
6. Khi user yêu cầu, agent **tạo pull request** vào `ducnm3`. Title = nội dung
   task backlog. Body đúng mẫu tiếng Việt trong [GIT_GUIDE.md](GIT_GUIDE.md)
   (Tổng quan, Trước chỉnh sửa, Sau chỉnh sửa, Nội dung chỉnh sửa, DB).
7. Agent review pull request **chỉ khi** user nhờ. User có thể merge luôn.

Chi tiết lệnh: [GIT_GUIDE.md](GIT_GUIDE.md). Checklist agent:
[`.agents/skills/developer-task/`](../../.agents/skills/developer-task/).

---

## 5. Bảng WBS đầy đủ

Số giờ dưới đây mang tính ví dụ từ task mẫu. Dev điền Est thật trên plan và
Jira.

### 5.1. Phase Estimate

| Vai trò | Hạng mục | Dev / SQA làm gì |
| --- | --- | --- |
| All / BE | Call transfer spec | Tiếp nhận spec; ghi link spec lên Jira và `docs/plan/` |
| BE | Tìm hiểu est | Đọc spec, điều tra code sơ bộ để estimate Design/Implement |
| SQA | Call transfer spec | SQA nhận spec, căn chỗ test |
| SQA | Tìm hiểu | SQA tìm hiểu phạm vi test để estimate |

**Dev kết thúc phase khi:** đã điền estimate các dòng dev liên quan; TL/BrSE đã
review tổng estimate trên task nếu team yêu cầu.

### 5.2. Phase Design

| Vai trò | Hạng mục | Dev phụ trách task làm gì |
| --- | --- | --- |
| BE | Tìm hiểu spec & QA | Đọc kỹ spec; note Q&A; tag BrSE; thảo luận SQA/FE nếu có |
| BE | PTYC & PVAH | Viết phân tích yêu cầu và phạm vi ảnh hưởng; link file trên Jira |
| BE | Design API | Thiết kế API nếu có; có thể gộp vào PVAH |

Trong MiniProject, PTYC/PVAH/Design API tương đương tài liệu trong `docs/`:

- PTYC: mục tiêu, luồng nghiệp vụ, AC → `docs/business-flows/`.
- PVAH: API, database, ảnh hưởng dữ liệu → `docs/api/`, `docs/database/`,
  `docs/architecture/`.
- Design API: endpoint, request/response → đúng một file trong
  `docs/api/<service>/endpoints/`.

**Output bắt buộc trên Jira:** link spec, link PTYC/PVAH (hoặc docs tương
đương), bảng Q&A, AC tóm tắt.

**Kết thúc phase khi:** TL approve tài liệu Design; SQA có thể bắt đầu
PTYC/Testcase song song ở Implement.

### 5.3. Phase Implement

**Nhánh Dev phụ trách task**

| Hạng mục | Dev phụ trách task làm gì |
| --- | --- |
| Coding | Code theo PTYC/PVAH đã duyệt; branch `feature/{mã backlog}` |
| Self test | Test API (Postman), case chính; checklist self-test |
| Review code | Khi user yêu cầu, tạo PR vào `ducnm3` đúng mẫu GIT_GUIDE; self-review; fix comment đến merge |

**Nhánh SQA** (dev cần biết để phối hợp, không thay SQA làm)

| Hạng mục | SQA làm gì |
| --- | --- |
| Tìm hiểu người làm task / người review | Nắm context dev và reviewer |
| PTYC | PTYC phía test (hoặc bổ sung) |
| TVP | Tài liệu kế hoạch verify test |
| Testcase | Viết / cập nhật testcase |
| Review / Update PTYC+TVP+TC | Review và cập nhật theo BrSE, comment KH |

Dev không chờ SQA xong mới Coding, nhưng phải có PTYC/PVAH (hoặc tài liệu thiết
kế tương đương) đã duyệt trước khi merge PR chính.

**Dev kết thúc nhánh Implement khi:** PR merged vào `ducnm3`; trên Jira có link
PR; self-test đã ghi nhận; SQA đủ tài liệu để vào Thực thi test.

### 5.4. Phase Thực thi test

**Nhánh R1**

| Hạng mục | Ai làm | Ghi chú |
| --- | --- | --- |
| Test R1 | SQA | Vòng test 1 |
| Verify Bug | SQA verify; **Dev fix** khi có bug | Dev ghi nguyên nhân / phạm vi / hướng fix |
| UT Dev | Dev | Unit test bổ sung nếu team yêu cầu trong R1 |
| Support test | Dev | Hỗ trợ SQA khi cần làm rõ behavior / env |

**Nhánh R2**

| Hạng mục | Ai làm | Ghi chú |
| --- | --- | --- |
| Test R2 | SQA | Vòng test 2 |
| Verify Bug | SQA verify; **Dev fix** | Tương tự R1 |

**Dev kết thúc phase test khi:** không còn bug Open do dev phụ trách; Verify
Bug Pass; đã support xong các ca SQA escalate.

**Done cả task khi:** các hạng mục trên bảng WBS đã hoàn thành; Summary khớp
thực tế; không còn blocker từ R2/KH.

---

## 6. Quy trình làm việc trên một task

Áp dụng cho **cùng một ticket** `【BLD-xxx】…`, không tạo sub-task rời theo
phase.

| Bước | Việc cần làm |
| --- | --- |
| 1 | Nhận task trên `docs/plan/` và Jira; xác nhận flag (ví dụ estimate VO hay không). |
| 2 | **Estimate** — Call transfer spec, Tìm hiểu est; điền Est; link spec. |
| 3 | **Design** — Tìm hiểu spec & QA → PTYC & PVAH → Design API (nếu có); nhờ TL review. |
| 4 | **Implement** — Coding trên `feature/{mã backlog}` → Self test → tạo PR vào `ducnm3` khi user yêu cầu; cập nhật Jira (link PR, checklist). |
| 5 | **Thực thi test** — Support test; fix bug (Verify Bug); UT Dev nếu có; lặp đến Pass R1 rồi R2. |
| 6 | **Done task** — Kiểm tra phụ lục attach; chuyển trạng thái Jira Done theo quy ước team. |

**Pattern lặp:** làm hạng mục → cập nhật tài liệu trên Jira và `docs/` → nhờ
review (TL / reviewer PR) → fix → đánh dấu xong hạng mục trên WBS.

---

## 7. Chuẩn attach tài liệu trên một task Jira

Toàn bộ context tập trung tại Description / comment / attachment của ticket
`【BLD-xxx】…`.

### 7.1. Nguyên tắc

- **Description** là mục lục: link tới spec, PTYC, PVAH, PR, checklist (repo
  `docs/`, Confluence, wiki).
- **Mỗi phase** bổ sung section tương ứng (không tạo ticket mới).
- Cùng **mã task** trên branch `feature/{mã backlog}` (`feature/BLD-124_4`).
  Commit message viết bằng tiếng Việt, rõ hành động và phạm vi thay đổi; ưu tiên
  dùng nội dung task backlog khi tiêu đề đã rõ nghĩa. Title Jira có thể giữ
  `【BLD-124_4】 …`.
- Khi SQA/BrSE/KH comment: trích vào Jira; dev cập nhật code + (nếu cần) bản
  PTYC/PVAH đã sửa.

### 7.2. Tài liệu bắt buộc theo phase (Dev)

| Phase trên WBS | Tối thiểu phải có trên Jira |
| --- | --- |
| Estimate | Link spec; ghi chú điều tra est; estimate đã điền |
| Design | Link PTYC, PVAH (hoặc `docs/` tương đương); Q&A; AC; kết quả review TL |
| Implement | Link PR do user tạo; kết quả self-test; trạng thái review/merge |
| Thực thi test | Với mỗi bug: nguyên nhân, phạm vi ảnh hưởng, hướng fix; link PR fix; Pass verify |

---

## 8. Chi tiết công việc Dev từng hạng mục

### 8.1. Estimate — Call transfer spec và Tìm hiểu est

- Nhận spec; đính link; xác nhận đã hiểu phạm vi với BrSE.
- Điều tra code liên quan; ghi module/API/file chính.
- Điền số giờ vào cột Est trên `docs/plan/` và dòng WBS; tham gia review tổng
  estimate task.

### 8.2. Design — Tìm hiểu spec & QA / PTYC & PVAH / Design API

- Q&A: note trên Jira, tag BrSE; không chốt PTYC khi Q&A critical còn Open.
- PTYC: mục tiêu, luồng nghiệp vụ, AC.
- PVAH: API, database, màn hình, ảnh hưởng dữ liệu.
- Design API: endpoint, request/response (có thể nằm trong PVAH).
- Nhờ TL review trước khi vào Implement.

### 8.3. Implement — Coding / Self test / Review code

- Coding theo PTYC/PVAH đã duyệt, đồng thời đọc skill HTTP-method / test /
  migration tương ứng trong `.agents/skills/`.
- Self test: Postman + unit/component/integration test theo skill test.
- Commit message viết bằng tiếng Việt, nêu rõ hành động và phạm vi thay đổi;
  ưu tiên nội dung task backlog khi tiêu đề đã rõ nghĩa.
- Khi user yêu cầu, agent tạo pull request vào `ducnm3` đúng mẫu tiếng Việt
  (Tổng quan / Trước chỉnh sửa / Sau chỉnh sửa / Nội dung chỉnh sửa / DB).
- Review code đến merge chỉ khi user nhờ agent; user có thể merge luôn.

### 8.4. Thực thi test — Support / Verify Bug / UT Dev

- **Support test:** trả lời SQA, cung cấp env, data, giải thích behavior.
- **Verify Bug:** điều tra, sửa trên nhánh ticket hoặc nhánh fix cùng mã;
  comment verify; lặp đến Pass.
- **UT Dev:** bổ sung unit test theo yêu cầu R1 nếu có estimate dòng này.

Ba mục bắt buộc khi ghi bug fix trên Jira:

1. **Nguyên nhân**
2. **Phạm vi ảnh hưởng**
3. **Hướng fix**

---

## 9. Phối hợp SQA trên cùng task

Trên phase Implement, SQA làm song song các dòng PTYC, TVP, Testcase. Dev không
chờ SQA xong mới Coding.

Trước Thực thi test, SQA cần:

- Testcase / TVP sẵn sàng (theo các dòng WBS SQA).
- Build/env từ dev phụ trách sau merge Implement.

Khi có comment BrSE/KH về tài liệu test: SQA cập nhật PTYC+TVP+TC; dev cập nhật
code/tài liệu Design nếu thay đổi phạm vi.

---

## 10. Map slide quy trình DEV và Jira

| Slide / tên cũ | Trên Jira task (phase / hạng mục) |
| --- | --- |
| Estimate | Phase Estimate (Dev: Tìm hiểu est) |
| Tìm hiểu spec, Q&A + PTYC/PVAH | Phase Design |
| Start coding | Phase Implement (Coding, Self test, Review code) |
| Fix bug R1 | Phase Thực thi test — R1: Verify Bug, Support test |
| Fix comment KH | SQA cập nhật PTYC+TVP+TC; Dev chỉnh code/tài liệu theo thống nhất |
| Fix bug R2 | Phase Thực thi test — R2: Test R2, Verify Bug |
