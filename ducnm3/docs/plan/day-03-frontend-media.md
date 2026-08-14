# NGÀY 3 — Hoàn thiện Media Service và hai trang FE

Est là giờ thật, **không** ép tổng 8 giờ/ngày.

Từ Ngày 3, mỗi hạng mục code có Task, Est và Ticket. `Chưa tạo` = cấm push
**code**. Markdown tài liệu push thẳng `ducnm3`. Nhánh code:
`feature/{mã backlog}`. Khi user yêu cầu, agent tạo pull request vào `ducnm3`.
Quy trình: [DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md).

## Ước lượng thời gian

| Task | Est | Ticket |
| --- | --- | --- |
| Hoàn thiện Media Service | 2 giờ | ERBUL26-2680 |
| FE: base Workbench và trang GET students | 1 giờ | ERBUL26-2681 |
| FE: trang POST media upload | 1 giờ | ERBUL26-2682 |

## Task dự kiến

### 1. Hoàn thiện Media Service

- [ ] Cài đặt `GET /api/media/usages/{usageId}/url` và
  `GET /api/media/usages/urls` (query owner): trả content URL, không lộ
  bucket/object key; `Cache-Control: no-store`.
- [ ] Unit/component test cho hai endpoint; đồng bộ API doc, business flow,
  Gateway route và Postman.
- Est: 2 giờ.
- Ticket: ERBUL26-2680.
- Nhánh: `feature/ERBUL26-2680`.

### 2. FE: base Workbench và trang GET students

- [ ] Chốt base `lms-web`: Vite, `AppShell`, Sidebar theo service, Workbench
  Input/Output, toast `httpClient`, theme Lucide. Gateway CORS cho origin
  Vite (`http://localhost:5173`).
- [ ] Trang đúng một API `GET /student/api/students`: constants, slice, hook,
  tab Mẫu/Thủ công, JSON/Xem/UML. Page không gọi Axios.
- Est: 1 giờ.
- Ticket: ERBUL26-2681.
- Nhánh: `feature/ERBUL26-2681`.

### 3. FE: trang POST media upload

- [ ] Trang đúng một API `POST /media/api/media`: menu Media, route
  `/media/upload`, `mediaApi` + slice mutation + `useMediaUpload`, form
  multipart, output JSON/Xem.
- [ ] Tái sử dụng Workbench/constants; không hard-code path/status.
- Est: 1 giờ.
- Ticket: ERBUL26-2682.
- Nhánh: `feature/ERBUL26-2682`.

## Tiêu chí hoàn thành của phạm vi này

- [ ] Media Service trả URL usage (một và theo owner) qua Gateway.
- [ ] `lms-web` gọi được `GET /student/api/students` (CORS + Workbench).
- [ ] `lms-web` upload được media qua `POST /media/api/media`.
