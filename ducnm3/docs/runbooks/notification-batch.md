# 18. Notification Batch — Naive Version

Version đầu tiên cố tình implement không tối ưu:

```csharp
var users = await studentClient.GetAllStudentsAsync();

foreach (var user in users)
{
    await sender.SendAsync(user);
}
```

Vấn đề cần ghi nhận:

- Load toàn bộ user vào RAM.
- HTTP request chạy lâu.
- Timeout.
- Không resume.
- Không retry tốt.
- Khó trace.
- Một lỗi có thể ảnh hưởng cả flow.

Đây là **Before** trong demo.

---
# 19. Notification Batch — Optimized Version

Các bước worker dưới đây là mục tiêu của phase execution, chưa được triển khai
trong Scheduler foundation.

Flow:

```text
POST /notification-batches
        │
        ▼
Create Notification Batch
        │
        ▼
202 Accepted
        │
        └───────────────── Client

Background Worker
        │
        ▼
Load 500 records
        │
        ▼
Create in-app notification
        │
        ▼
Update Batch Items
        │
        ▼
Next Batch
```

Recommended:

```text
batchSize = 500
```

Có thể test:

```text
100
500
1000
```

để có số liệu trade-off.

---
# 20. Retry Strategy

Rule:

```text
Attempt 1
   │
   ├── Success
   │
   └── Fail
        │
        ▼
     Retry 1
        │
        ├── Success
        │
        └── Fail
             │
             ▼
           FAILED
```

Lưu:

```text
retry_count
error_message
processed_at
status
```

---
# 21. Fake Notification Sender để demo lỗi

Không cần tích hợp email/SMS thật.

Có thể tạo:

```text
FakeNotificationSender
```

Behavior:

```text
5% request fail random
```

hoặc fail theo rule:

```text
studentId % 20 == 0
```

Ưu điểm:

- Demo deterministic.
- Không phụ thuộc dịch vụ ngoài.
- Có thể show retry rõ ràng.

---
# 22. Idempotency

Tình huống demo:

```text
Batch xử lý student 1 → 500
Worker crash
Worker restart
```

Nếu không idempotent:

```text
student 1 → 500
có thể nhận notification lần 2
```

Fix:

```text
UNIQUE(batch_id, student_id)
UNIQUE(notification_batch_id, recipient_student_id)
```

và chỉ process:

```text
status IN (PENDING, RETRY)
```

Nếu:

```text
status = SUCCESS
```

thì skip.

Khi retry, handler phải tái sử dụng hoặc kiểm tra notification đã tạo cho
`batch_id + student_id` trước khi insert, để học viên không thấy hai inbox item
giống nhau. Scheduler run dùng idempotency key riêng và không thay thế
constraint nghiệp vụ này.

---
