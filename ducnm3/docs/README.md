# Tài liệu

Thư mục này chứa kế hoạch về các vi dịch vụ LMS, được sắp xếp theo chủ đề. Hãy xem các lựa chọn kỹ thuật là đề xuất cho đến khi dự án chính thức áp dụng.

## Bắt đầu tại đây

- [Tổng quan dự án](overview.md)
- [Đề xuất kiến trúc](architecture/microservices.md)
- [Ngăn xếp công nghệ](development/tech-stack.md)
- [Kế hoạch triển khai trong năm ngày](plan/)

## Kiến trúc

- `architecture/microservices.md`: ranh giới dịch vụ, quyền sở hữu cơ sở dữ liệu và trách nhiệm.
- `architecture/clean-architecture.md`: kiến trúc sạch và cấu trúc thư mục dịch vụ.
- `architecture/rich-content-and-media.md`: quyền sở hữu của Media Service, nội dung Markdown và vòng đời sử dụng nội dung đa phương tiện.
- `architecture/uml.md`: các sơ đồ UML bắt buộc.
- `architecture/conclusion.md`: bản tổng kết kiến trúc cuối cùng.

## Luồng nghiệp vụ

- `business-flows/`: quản lý khóa học, sử dụng nội dung đa phương tiện, ghi danh, thông báo đơn lẻ/hàng loạt, hộp thư đến và nội dung đa phương tiện của thông báo.

## API và dữ liệu

- `api/`: các hợp đồng API được nhóm theo Course, Student, Media, Notification và Scheduler Service.
- `api/error-handling-observability.md`: hợp đồng lỗi, ghi nhật ký và khả năng quan sát.
- `database/lms-data-model.md`: các thực thể LMS, nội dung đa phương tiện, tác vụ thông báo và hộp thư đến của học viên.

## Phát triển và vận hành

- `development/`: các hướng dẫn về ngăn xếp công nghệ, MinIO, Docker, hiệu năng, bàn giao và chuẩn bị.
- `guide/`: các hướng dẫn thiết lập và vận hành thực tế, bao gồm Docker Compose và Swagger dùng chung.
- `runbooks/notification-batch.md`: xử lý theo lô, thử lại, tính lũy đẳng và xử lý lỗi.
- `runbooks/demo-script.md`: quy trình trình diễn trong 30 phút.

## Kế hoạch hằng ngày

- `plan/day-01-foundation-docker-clean-architecture.md`
- `plan/day-02-lms-core-minio-n-1.md`
- `plan/day-03-batch-retry-idempotency.md`
- `plan/day-04-performance-day.md`
- `plan/day-05-error-handling-test-slide-demo.md`

Cập nhật tài liệu liên quan trong cùng một thay đổi mỗi khi phần triển khai làm thay đổi hành vi hoặc quyết định đã được ghi lại.
