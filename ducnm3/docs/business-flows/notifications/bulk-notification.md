# Thông báo hàng loạt

## Mục đích

Quản trị viên tạo một lô và snapshot tất cả học viên đang hoạt động. Notification Worker xử lý lô bất đồng bộ; API không gửi inbox trong HTTP request.

## Tác nhân

Quản trị viên; Notification API; Student Service; MySQL Notification; RabbitMQ; Notification Worker.

## Điều kiện đầu vào

- `targetScope` là `ALL_STUDENTS`.
- `createdBy`, `title` và `bodyMarkdown` hợp lệ.
- Student Service trả về snapshot không rỗng.

## UML luồng chạy

### `POST /api/notification-batches`

```mermaid
sequenceDiagram
    participant Admin
    participant API as Notification API
    participant Student as Student Service
    participant DB as MySQL Notification
    participant Bus as RabbitMQ

    Admin->>API: POST batch (ALL_STUDENTS)
    API->>API: Validate body, scope, createdBy
    API->>Student: GET /api/students?status=ACTIVE&pageSize=100
    loop hết các trang
        Student-->>API: student ids + totalPages
    end
    alt Scope sai hoặc snapshot rỗng
        API-->>Admin: 400 VALIDATION_FAILED
    else Student Service không phản hồi
        API-->>Admin: 503 STUDENT_SERVICE_UNAVAILABLE
    else Hợp lệ
        API->>DB: INSERT batch PENDING + batch items
        API->>Bus: DispatchNotificationBatchV1(batchId)
        API-->>Admin: 202 + Location GET batch
    end
```

### `GET /api/notification-batches/{batchId}`

```mermaid
sequenceDiagram
    participant Client
    participant API as Notification API
    participant DB as MySQL Notification

    Client->>API: GET batchId
    API->>DB: SELECT batch counters and status
    alt Không tìm thấy
        API-->>Client: 404 NOTIFICATION_BATCH_NOT_FOUND
    else Tìm thấy
        DB-->>API: batch summary
        API-->>Client: 200 batch summary
    end
```

### `DispatchNotificationBatchV1` (Notification Worker)

```mermaid
sequenceDiagram
    participant Bus as RabbitMQ
    participant Worker as Notification Worker
    participant DB as MySQL Notification
    participant Sender as FakeNotificationSender

    Bus->>Worker: DispatchNotificationBatchV1(batchId)
    Worker->>DB: Claim tối đa batchSize item PENDING/RETRY
    loop từng item trong chunk
        Worker->>Sender: Send(studentId, retryCount + 1)
        alt Gửi thành công
            Worker->>DB: INSERT notification BULK/UNREAD + item SUCCESS
        else Lần 1 thất bại
            Worker->>DB: item RETRY, retry_count = 1
        else Lần 2 thất bại
            Worker->>DB: item FAILED + error_message
        end
    end
    Worker->>DB: Update counters, kiểm tra item còn lại
    alt Còn PENDING/RETRY
        Worker->>Bus: DispatchNotificationBatchV1(batchId)
    else Đã xong
        Worker->>DB: COMPLETED / PARTIAL_FAILED / FAILED
    end
```

## Quy tắc xử lý

1. Snapshot dùng danh sách Student Service trả tại lúc POST; Worker không gọi lại Student Service.
2. Một chunk có tối đa `batchSize` item, mặc định 500; Worker không tải toàn bộ lô vào bộ nhớ. Consumer xử lý tuần tự một message; item `PROCESSING` còn lại sau khi process bị dừng được claim lại ở lượt dispatch tiếp theo.
3. Fake sender thất bại lần một khi `hash(studentId) % 20 == 0`, và lần hai khi `hash(studentId) % 100 == 0`.
4. Item `SUCCESS` không được xử lý lại. Unique `(batch_id, student_id)` và `(notification_batch_id, recipient_student_id)` bảo vệ dữ liệu nghiệp vụ khỏi trùng lặp.
5. Khi không còn item: không có lỗi là `COMPLETED`; chỉ lỗi là `FAILED`; có cả thành công và lỗi là `PARTIAL_FAILED`.

## Dữ liệu thay đổi

- Notification DB: `notification_batches`, `notification_batch_items`, `notifications`.
- Student Service chỉ đọc danh sách học viên.
- Không có Scheduler Service hoặc `background_jobs` trong flow này.
