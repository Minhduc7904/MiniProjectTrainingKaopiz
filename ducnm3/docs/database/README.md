# Tài liệu cơ sở dữ liệu

Mỗi service sở hữu database riêng; không tạo foreign key xuyên service. Data
model được tách theo ownership để migration, ERD và bảng MassTransit không bị
lẫn giữa các service:

- [Course Service](course-service/data-model.md)
- [Student Service](student-service/data-model.md)
- [Media Service](media-service/data-model.md)
- [Notification Service](notification-service/data-model.md)
- [Scheduler Service](scheduler-service/data-model.md)

Tài liệu từng service ghi bảng nghiệp vụ, kiểu cột, ràng buộc, chỉ mục, ERD và
trạng thái `InboxState`/`OutboxState`/`OutboxMessage` nếu service đó có dùng
MassTransit persistence. SQL migration vẫn là nguồn sự thật cho schema đã triển
khai; không sửa migration đã có trong `schema_migrations`.
