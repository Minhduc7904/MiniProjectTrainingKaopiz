# Quy tắc giao tiếp giữa các service

- QUERY cần response ngay dùng typed HTTP client qua `BuildingBlocks.Http`.
- COMMAND không cần kết quả ngay implement `ICommand` và dùng
  `ICommandSender.SendAsync`; không `Publish` command.
- EVENT đã xảy ra implement `IIntegrationEvent` và dùng
  `IEventPublisher.PublishAsync`; mỗi subscriber phải có queue riêng.
- Message contract phải thuộc service owner, có tên rõ nghĩa và chỉ thay đổi
  theo hướng backward-compatible; breaking change cần contract version mới.
- Consumer phải idempotent vì delivery là at-least-once.
- Mọi consumer nhận retry policy từ `Messaging:Retry` qua
  `AddLmsMessagingWithConsumers(...)`. Không hard-code hoặc override retry trong
  consumer nếu chưa có architecture decision được ghi trong docs.
- Sau khi hết retry, giữ message trong MassTransit `_error` queue để điều tra;
  không tạo vòng requeue vô hạn.
- Queue command dùng `AddCommandConsumer`; queue event dùng `AddEventConsumer`.
  Không tự đặt queue name khác convention.
- Forward `X-Correlation-Id`; log correlation, message type và attempt nhưng
  không log secret hoặc payload nhạy cảm.
- Không gửi message nghiệp vụ quan trọng trong cùng use case ghi database cho
  tới khi đã có Outbox/Inbox hoặc giải pháp chống dual-write tương đương.
- Không dùng RabbitMQ để thực hiện QUERY và không truy cập database của service
  khác.
