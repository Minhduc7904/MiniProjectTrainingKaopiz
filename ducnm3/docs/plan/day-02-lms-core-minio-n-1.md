# NGÀY 2 — LMS core, Media API, giao tiếp service và dữ liệu development

## Kết quả thực tế

Ngày 2 gồm 5 task, được tách theo lịch sử thay đổi trên nhánh `ducnm3` ngày
13/08/2026: ba pull request đã merge và hai commit được push trực tiếp. Phạm vi
đã hoàn thành tập trung vào dữ liệu development, giao tiếp giữa service và Media
API; CRUD Course/Lesson và benchmark N+1 vẫn chưa xuất hiện trong lịch sử ngày
này.

## Ước lượng thời gian

Ước lượng được làm tròn theo mốc 30 phút và phản ánh phạm vi task đã tách,
không phải thời gian thực tế của pull request hoặc commit.

| Task | Nguồn | Ước lượng |
| --- | --- | --- |
| Deterministic development data seeder | PR #143 | 1 giờ 30 phút |
| Việt hóa tài liệu project | commit `69af2aa2` | 1 giờ |
| Nền tảng HTTP và RabbitMQ | PR #144 | 1 giờ 30 phút |
| Media upload/content và Student avatar usage | PR #145 | 2 giờ 30 phút |
| Workflow agent, API artifact và Postman | commit `518e9618` | 1 giờ 30 phút |
| **Tổng** |  | **8 giờ** |

## Task đã hoàn thành

### 1. Tạo deterministic development data seeder

- [x] Thêm công cụ `Lms.DataSeeder`, script chạy seed và service Docker để tạo
  dữ liệu development có thể lặp lại.
- [x] Bổ sung unit/integration test, tài liệu data-seed, hướng dẫn Docker và
  runbook reset database.
- Ước lượng: 1 giờ 30 phút.
- Nguồn: [PR #143 — add deterministic development data seeder](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/143/overview).

### 2. Việt hóa tài liệu project

- [x] Chuẩn hóa tài liệu hiện có sang tiếng Việt, đồng thời giữ các thuật ngữ kỹ
  thuật phổ biến để bảo toàn ngữ cảnh triển khai.
- Ước lượng: 1 giờ.
- Nguồn: [commit `69af2aa2` — localize project documentation in Vietnamese](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/commits/69af2aa2d5536af42453795bf95c67d4574c79cc).

### 3. Xây nền tảng HTTP và RabbitMQ cho giao tiếp service

- [x] Thêm Building Blocks cho HTTP client có correlation ID và truy vấn service
  nội bộ.
- [x] Thêm abstraction/implementation RabbitMQ-MassTransit, cấu hình transport,
  consumer registration, message contract và messaging health probe.
- [x] Bổ sung Docker Compose, tài liệu và unit/integration test cho communication
  foundation.
- Ước lượng: 1 giờ 30 phút.
- Nguồn: [PR #144 — add HTTP and RabbitMQ communication foundation](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/144/overview).

### 4. Hoàn thiện Media upload/content và liên kết avatar Student

- [x] Cài đặt upload media, lưu metadata `media_objects`, đọc nội dung dạng
  stream và tạo `media_usages`.
- [x] Thêm kiểm tra Student qua HTTP client để liên kết avatar, migration cho
  media upload lifecycle và test unit/component/integration cho luồng này.
- [x] Bổ sung `GET /students/{id}` để Media Service xác thực Student owner.
- Ước lượng: 2 giờ 30 phút.
- Nguồn: [PR #145 — add Media upload and Student avatar usage APIs](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/145/overview).

### 5. Chuẩn hóa workflow agent, API artifact và Postman collection

- [x] Thêm workflow skill theo HTTP method, unit/component/integration test và
  database migration.
- [x] Chuẩn hóa API endpoint template, tách business flow theo endpoint và thêm
  Postman collection/hướng dẫn sử dụng.
- Ước lượng: 1 giờ 30 phút.
- Nguồn: [commit `518e9618` — add agent workflow skills and API artifacts](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/commits/518e9618f0c161e1cda4d3565e9acd0fa97884c3).

## Tiêu chí hoàn thành Ngày 2

- [x] Có deterministic development data seeder và tài liệu/test đi kèm.
- [x] Media Service hỗ trợ upload, streaming content, lưu metadata và liên kết
  usage cho avatar Student.
- [x] Có foundation HTTP/RabbitMQ cho giao tiếp giữa service.
- [x] Workflow tài liệu, test, migration và Postman đã được chuẩn hóa.

## Công việc chưa hoàn thành trong lịch sử Ngày 2

- [ ] CRUD Course và Lesson.
- [ ] Ghi danh và tiến độ học tập.
- [ ] Liên kết media cho Course/Lesson.
- [ ] `GET /courses/details-naive` và `GET /courses/details-optimized`.
- [ ] SQL log và benchmark để so sánh N+1 với truy vấn đã tối ưu.
