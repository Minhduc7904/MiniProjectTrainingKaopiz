# NGÀY 3 — Triển khai base FE và hoàn thiện Media Service

Ngày 2 đã có khung `lms-web` (trang Sổ học viên), Media upload/content/usage
avatar và thumbnail pipeline. Ngày 3: hoàn thiện **base** frontend và khép
usage/cleanup của Media Service.

Est là giờ làm việc thật của từng hạng mục, **không** chỉnh cho khớp 8 giờ/ngày.

Từ Ngày 3, mỗi hạng mục code có Task, Est và Ticket. `Chưa tạo` = cấm push
**code**. Markdown tài liệu push thẳng `ducnm3`. Nhánh code:
`feature/{mã backlog}`. Khi user yêu cầu, agent tạo pull request vào `ducnm3`.
Quy trình: [DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md).

## Ước lượng thời gian

| Task | Est | Ticket |
| --- | --- | --- |
| Triển khai base FE | 2 giờ | `Chưa tạo` |
| Hoàn thiện Media Service | 3 giờ | `Chưa tạo` |

## Task dự kiến

### 1. Triển khai base FE

- [ ] Chốt base `lms-web` theo
  [docs/architecture/frontend.md](../architecture/frontend.md) và skill
  `interface-design`: `AppShell`, router, constants, `api` + slice + hook +
  page pattern. Page không gọi Axios; UI không biết Redux.
- [ ] Tái sử dụng toast interceptor và component UI hiện có (`Field`,
  `Dropdown`, `Pagination`, `Skeleton`). Không hard-code path/status trong page.
- [ ] Cập nhật tài liệu frontend nếu thêm route/env.
- Est: 2 giờ.
- Ticket: `Chưa tạo`.
- Nguồn: khung Students, AppShell và toast đã có; task này chỉ hoàn thiện base,
  không làm trang Media đầy đủ.

### 2. Hoàn thiện Media Service

- [ ] Mở command tạo `media_usages` cho tổ hợp Course/Lesson/Notification còn
  thiếu (`THUMBNAIL` / `EMBED` / `ATTACHMENT`), đúng unique active và soft-delete
  khi thay thế. Hiện command mới chấp nhận avatar Student.
- [ ] Scheduler/Worker dọn `PENDING` stale và media không còn usage hoạt động
  (job cleanup đã mô tả trong business flow, chưa triển khai).
- [ ] Unit/component/integration test cho usage mới và cleanup; đồng bộ
  `docs/api/media-service/`, business flow, database và Postman.
- Est: 3 giờ.
- Ticket: `Chưa tạo`.
- Nguồn: Day 2 còn Course/Lesson media link và cleanup PENDING.

## Tiêu chí hoàn thành của phạm vi này

- [ ] Base `lms-web` (layout, router, pattern page) sẵn sàng để thêm trang sau.
- [ ] Media Service tạo được usage Course/Lesson/Notification theo contract.
- [ ] Cleanup stale PENDING / orphan media chạy được và có test.
