# NGÀY 3 — Media, FE và notification batch

Ba hạng mục Media/FE đã có mã ticket. Batch Job dùng `ERBUL26-2690`. Batch
Performance Ticket `Chưa tạo` = cấm push code. Markdown push thẳng `ducnm3`.
Est là giờ thật. Quy trình:
[DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md).

## Ước lượng thời gian

| Task | Est | Ticket |
| --- | --- | --- |
| Hoàn thiện Media Service | 2 giờ | `ERBUL26-2680` |
| FE: base Workbench và trang GET students | 1 giờ | `ERBUL26-2681` |
| FE: trang POST media upload | 1 giờ | `ERBUL26-2682` |
| Batch Job — gửi notification ~3k | 4 giờ | `ERBUL26-2690` |

## Task đã có ticket

### 1. Hoàn thiện Media Service

- [ ] Cài đặt `GET /api/media/usages/{usageId}/url` và
  `GET /api/media/usages/urls` (query owner): trả content URL, không lộ
  bucket/object key; `Cache-Control: no-store`.
- [ ] Unit/component test cho hai endpoint; đồng bộ API doc, business flow,
  Gateway route và Postman.
- Est: 2 giờ.
- Ticket: `ERBUL26-2680`.
- Nhánh: `feature/ERBUL26-2680`.
- Nguồn: Day 2 đã có upload, content stream, avatar usage và thumbnail.
- Pull request:
  - [#151 — Hoàn thiện Media Service](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/151)

### 2. FE: base Workbench và trang GET students

- [ ] Chốt base `lms-web`: Vite, `AppShell`, Sidebar theo service, Workbench
  Input/Output, toast `httpClient`, theme Lucide. Gateway CORS cho origin
  Vite (`http://localhost:5173`).
- [ ] Trang đúng một API `GET /student/api/students`: constants, slice, hook,
  tab Mẫu/Thủ công, JSON/Xem/UML. Page không gọi Axios.
- Est: 1 giờ.
- Ticket: `ERBUL26-2681`.
- Nhánh: `feature/ERBUL26-2681`.
- Nguồn: `pages/students/`, CORS Gateway, skill `frontend-api-page`.
- Pull request:
  - [#152 — FE: base Workbench và trang GET students](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/152)

### 3. FE: trang POST media upload

- [ ] Trang đúng một API `POST /media/api/media`: menu Media, route
  `/media/upload`, `mediaApi` + slice mutation + `useMediaUpload`, form
  multipart, output JSON/Xem.
- [ ] Tái sử dụng Workbench/constants; không hard-code path/status.
- Est: 1 giờ.
- Ticket: `ERBUL26-2682`.
- Nhánh: `feature/ERBUL26-2682`.
- Nguồn: `pages/media/`, `api/mediaApi.js`, `constants/activities/postMedia.js`.
- Pull request:
  - [#153 — FE: trang POST media upload](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/153)

## Task tiếp theo: notification batch

Schema `notification_batches` / `notification_batch_items` đã có. **Chưa có**
POST, Worker dispatch, retry trong code.

### 4. Batch Job — gửi notification cho ~3k user

- [ ] `POST /api/notification-batches`: `targetScope=ALL_STUDENTS`, snapshot
  recipient từ `GET /api/students` (phân trang), ghi batch `PENDING` + items,
  trả `202 Accepted` + `Location`. Không gửi notification trong HTTP request.
- [ ] `NotificationService.Worker` nhận command chỉ có `batchId`; mỗi lượt tối
  đa `batchSize` (500) item `PENDING`/`RETRY`; tạo `notifications`; cập nhật
  counter. Idempotent `UNIQUE(batch_id, student_id)` và
  `UNIQUE(notification_batch_id, recipient_student_id)`.
- [ ] Retry là yêu cầu của batch (không tách ticket): item lỗi → `RETRY` đúng
  1 lần; vẫn lỗi → `FAILED` + `error_message`. Fake sender, không email/SMS.
- [ ] `GET /api/notification-batches/{batchId}` để poll trạng thái (Location
  của 202).
- [ ] Worker claim item bằng lease và `SKIP LOCKED`, dispatch theo chunk với
  concurrency cấu hình được để tránh xử lý trùng khi scale worker.
- Est: 4 giờ.
- Ticket: `ERBUL26-2690`.
- Nhánh: `feature/ERBUL26-2690`.
- Lý do est: schema sẵn; API snapshot + Worker chunk + nhánh retry/fail trong
  cùng handler; bổ sung lease/concurrency cho worker. Một `targetScope` đủ cho
  demo 3k.
- Pull request:
  - [#154 — Batch Job — gửi notification ~3k](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/154)
  - [#158 — Feature/ERBUL26-2690](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/158)
  - [#159 — Feature/ERBUL26-2690](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/159)

## Tiêu chí hoàn thành của phạm vi này

- [ ] Media Service trả URL usage (một và theo owner) qua Gateway.
- [ ] `lms-web` gọi được `GET /student/api/students` (CORS + Workbench).
- [ ] `lms-web` upload được media qua `POST /media/api/media`.
- [ ] `POST /notification-batches` trả 202; Worker gửi theo chunk, không trong
  HTTP request.
- [ ] Item lỗi retry 1 lần; fail cuối ghi `FAILED` + `error_message`.
