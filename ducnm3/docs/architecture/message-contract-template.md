# Template cho message contract

Chỉ tạo contract khi có use case thật. Đặt contract trong project contract của
service owner; không đặt entity hoặc Infrastructure type vào payload.

## Thông tin cần ghi trong docs

```text
Message name:
Kind: COMMAND | EVENT
Owner:
Producer:
Consumer/subscriber:
Schema version:
Idempotency key:
Sensitive fields:
Expected retry behavior:
Failure handling:
Backward compatibility:
```

## C# shape tham khảo

COMMAND:

```csharp
public sealed record DispatchNotificationBatchV1(
    Guid BatchId,
    DateTimeOffset RequestedAtUtc) : ICommand;
```

EVENT:

```csharp
public sealed record CoursePublishedV1(
    Guid CourseId,
    DateTimeOffset OccurredAtUtc) : IIntegrationEvent;
```

Ví dụ chỉ minh họa shape và versioning, không phải contract đã được triển khai.
Không tái sử dụng database entity làm message payload.

## Checklist review

- COMMAND có đúng một owner và được `Send`.
- EVENT mô tả việc đã xảy ra và được `Publish`.
- Field bắt buộc có ý nghĩa ổn định; thay đổi breaking dùng version mới.
- Consumer có idempotency strategy.
- Payload không chứa secret hoặc dữ liệu thừa.
- Use case ghi database và publish message quan trọng đã có Outbox/Inbox.
- Retry lấy từ centralized `Messaging:Retry`, không đặt trong consumer.
