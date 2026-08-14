# Developer Task Reference

## Phạm vi ngày

- Ngày 1–2: lịch sử trên `ducnm3`. Không bắt buộc skill này.
- Ngày 3–5: bắt buộc skill này cho mọi hạng mục trên `docs/plan/`.

## Cột plan

| Cột | Quy ước |
| --- | --- |
| Task | Một hạng mục = một ticket Jira |
| Est | Estimate giờ thật; không bắt buộc tổng ngày = 8 giờ |
| Ticket | Mã Jira (`BLD-124_4`). `Chưa tạo` hoặc trống = cấm push code |

User tạo ticket. Agent không gọi Jira API, không tạo issue, không đặt tên ticket
hộ user. Khi user gửi mã, cập nhật cột Ticket trên file plan tương ứng và trên
mỗi mục chi tiết.

## Nhánh và commit

```text
ducnm3                  ← nhánh tích hợp; PR merge vào đây
feature/BLD-124_4       ← nhánh task; feature/{mã backlog}
```

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git switch -c feature/BLD-124_4
```

- Tên nhánh = `feature/{mã backlog}`, không phải `【BLD-124_4】 Đối ứng gửi mail…`.
- Không dùng `feature/ducnm3_<short-description>` từ Ngày 3.
- Commit message = nội dung task trên backlog, tiếng Việt:

```text
Triển khai base FE
Hoàn thiện Media Service
```

## Cổng push

Push **code** khi **cả hai** đúng:

1. Cột Ticket của hạng mục đang làm khác `Chưa tạo` và không trống.
2. User yêu cầu push rõ.

Nếu thiếu một trong hai: không `git push` code. Có thể commit local. Nói rõ lý
do: chưa có mã ticket trên plan.

**Ngoại lệ tài liệu markdown:** diff chỉ gồm `docs/**/*.md`, `AGENTS.md`,
`rules/*.md`, `.agents/**/*.md` thì commit và push thẳng `ducnm3`. Không cần
ticket branch, không bắt buộc PR. Lý do: sửa docs không đổi hành vi code.

Không `--no-verify`, không force-push.

## Pull request

Khi user yêu cầu tạo PR:

1. Push `feature/{mã backlog}` (cổng ticket đã mở).
2. Tạo PR target `ducnm3`.
3. Title = nội dung task trên backlog.
4. Body **bắt buộc** đúng mẫu, tiếng Việt, thuật ngữ kỹ thuật giữ tiếng Anh.
   Không bỏ section. **DB** ghi `Không có` nếu không đổi schema/migration.

```text
Mô tả:
Tổng quan:
<tóm tắt mục đích thay đổi và phạm vi>

Trước chỉnh sửa:
<hành vi / API / UI / schema hiện tại>

Sau chỉnh sửa:
<hành vi / API / UI / schema sau khi merge>

Nội dung chỉnh sửa:
<các thay đổi chính: file, endpoint, luồng>

DB (nếu có):
<migration, table, column, index, constraint — hoặc "Không có">
```

- Agent review PR chỉ khi user nhờ. User có thể merge không qua review.
- Khi review: đọc diff PR, đối chiếu PTYC/PVAH/`docs/`, test evidence, không
  merge hộ trừ khi user yêu cầu merge rõ.

## Map PTYC / PVAH → docs

| Artifact công ty | Trong repo |
| --- | --- |
| Spec KH | Link trên Jira Description; trích trên `docs/plan/` |
| PTYC (mục tiêu, luồng, AC) | `docs/business-flows/` |
| PVAH (API, DB, ảnh hưởng) | `docs/api/`, `docs/database/`, `docs/architecture/` |
| Design API | `docs/api/<service>/endpoints/` — đúng một file / endpoint |
| Self-test | Postman `docs/` hoặc collection hiện có; test catalog `docs/tests/` |
| PR | Link PR trên Jira sau khi tạo |

Đọc thêm skill HTTP-method / test / migration trước khi sửa code thuộc loại đó.

## WBS — việc thuộc Dev

| Phase | Hạng mục Dev | Kết thúc khi |
| --- | --- | --- |
| Estimate | Call transfer spec, Tìm hiểu est | Est đã điền (giờ thật); link spec |
| Design | Spec & QA, PTYC & PVAH, Design API | Docs đã cập nhật; Q&A critical không còn Open |
| Implement | Coding, Self test, Review code | PR merged; self-test ghi nhận |
| Thực thi test | Support test, Verify Bug, UT Dev | Không còn bug Open do dev; ba mục fix đã ghi |

SQA / R1 / R2 / TVP / testcase là việc SQA. Dev phối hợp, không làm thay.

## Bug fix trên Jira

Mỗi lần Verify Bug, comment đủ:

1. Nguyên nhân
2. Phạm vi ảnh hưởng
3. Hướng fix

Kèm link PR fix nếu đã có PR.
