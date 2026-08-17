# Tài liệu

Thư mục này chứa kế hoạch về các vi dịch vụ LMS, được sắp xếp theo chủ đề. Hãy xem các lựa chọn kỹ thuật là đề xuất cho đến khi dự án chính thức áp dụng.

## Bắt đầu tại đây

- [Tổng quan dự án](overview.md)
- [Kiến trúc hệ thống](architecture/README.md)
- [Current Tech Stack](development/tech-stack.md)
- [Project Phase 0–2](project/README.md)
- [Kế hoạch triển khai trong năm ngày](plan/)

## Kiến trúc

- `architecture/backend/`: kiến trúc Backend dùng chung và năm service theo ownership.
- `architecture/frontend/architecture.md`: SPA React, Redux, Axios, Gateway và rule page/component/hook.
- `architecture/README.md`: mục lục và đường dẫn đọc toàn bộ kiến trúc.

## Luồng nghiệp vụ

- `business-flows/`: quản lý khóa học, sử dụng nội dung đa phương tiện, ghi danh, thông báo đơn lẻ/hàng loạt, hộp thư đến và nội dung đa phương tiện của thông báo.

## API và dữ liệu

- `api/`: các hợp đồng API được nhóm theo Course, Student, Media, Notification và Scheduler Service.
- `api/shared/error-handling-observability.md`: error contract, logging và observability.
- `database/README.md`: data model và ERD được tách theo Course, Student, Media, Notification và Scheduler Service.

## Phát triển và vận hành

- `development/`: các hướng dẫn về ngăn xếp công nghệ, MinIO, Docker, hiệu năng, bàn giao và chuẩn bị.
- `guide/`: các hướng dẫn thiết lập và vận hành thực tế, bao gồm Docker Compose và Swagger dùng chung.
- `guide/DEVKIT_GUIDE.md`: cách dùng DevKit CLI, task session, task runner và MCP tools trong Cursor, Claude và Codex.
- `runbooks/notification-batch.md`: xử lý theo lô, thử lại, tính lũy đẳng và xử lý lỗi.
- `runbooks/demo-script.md`: quy trình trình diễn trong 30 phút.

## Kế hoạch hằng ngày

- `plan/day-01.md`
- `plan/day-02.md`
- `plan/day-03.md`
- `plan/day-04.md`
- `plan/day-05.md`

Cập nhật tài liệu liên quan trong cùng một thay đổi mỗi khi phần triển khai làm thay đổi hành vi hoặc quyết định đã được ghi lại.
