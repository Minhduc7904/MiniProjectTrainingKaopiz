# 18. Thông báo hàng loạt — Phiên bản đơn giản

Phiên bản đầu tiên được chủ ý triển khai chưa tối ưu:

```csharp
var users = await studentClient.GetAllStudentsAsync();

foreach (var user in users)
{
    await sender.SendAsync(user);
}
```

Vấn đề cần ghi nhận:

- Nạp toàn bộ người dùng vào RAM.
- Yêu cầu HTTP chạy lâu.
- Hết thời gian chờ.
- Không thể tiếp tục.
- Cơ chế thử lại chưa tốt.
- Khó theo dõi.
- Một lỗi có thể ảnh hưởng cả luồng.

Đây là trạng thái **Trước** trong phần trình diễn.

---
# 19. Thông báo hàng loạt — Phiên bản tối ưu

Các bước của tiến trình xử lý nền dưới đây là mục tiêu của giai đoạn thực thi, chưa được triển
khai trong nền tảng Scheduler.

Luồng:

```text
POST /notification-batches
        │
        ▼
Tạo lô thông báo
        │
        ▼
202 Accepted
        │
        └───────────────── Máy khách

Tiến trình xử lý nền
        │
        ▼
Nạp 500 bản ghi
        │
        ▼
Tạo thông báo trong ứng dụng
        │
        ▼
Cập nhật các mục trong lô
        │
        ▼
Lô tiếp theo
```

Khuyến nghị:

```text
batchSize = 500
```

Có thể kiểm thử:

```text
100
500
1000
```

để có số liệu về sự đánh đổi.

---
# 20. Chiến lược thử lại

Quy tắc:

```text
Lần thử 1
   │
   ├── Thành công
   │
   └── Thất bại
        │
        ▼
     Lần thử lại 1
        │
        ├── Thành công
        │
        └── Thất bại
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
# 21. Bộ gửi thông báo giả để trình diễn lỗi

Không cần tích hợp email/SMS thật.

Có thể tạo:

```text
FakeNotificationSender
```

Hành vi:

```text
5% yêu cầu thất bại ngẫu nhiên
```

hoặc thất bại theo quy tắc:

```text
studentId % 20 == 0
```

Ưu điểm:

- Phần trình diễn có tính xác định.
- Không phụ thuộc dịch vụ ngoài.
- Có thể trình bày cơ chế thử lại rõ ràng.

---
# 22. Tính lũy đẳng

Tình huống trình diễn:

```text
Lô xử lý học viên 1 → 500
Tiến trình xử lý nền gặp sự cố
Tiến trình xử lý nền khởi động lại
```

Nếu không có tính lũy đẳng:

```text
học viên 1 → 500
có thể nhận thông báo lần 2
```

Cách sửa:

```text
UNIQUE(batch_id, student_id)
UNIQUE(notification_batch_id, recipient_student_id)
```

và chỉ xử lý:

```text
status IN (PENDING, RETRY)
```

Nếu:

```text
status = SUCCESS
```

thì bỏ qua.

Khi thử lại, bộ xử lý phải tái sử dụng hoặc kiểm tra thông báo đã tạo cho
`batch_id + student_id` trước khi chèn, để học viên không thấy hai mục hộp thư
đến giống nhau. Lần chạy Scheduler dùng khóa lũy đẳng riêng và không thay thế
ràng buộc nghiệp vụ này.

---
