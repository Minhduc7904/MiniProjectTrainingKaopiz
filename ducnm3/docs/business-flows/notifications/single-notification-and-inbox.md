# Thông báo đơn lẻ và hộp thư đến của Học viên

## Mục đích

Quản trị viên gửi một thông báo cho một Học viên; Học viên chỉ xem và đánh dấu đã đọc thông báo của chính mình.

## Tác nhân

Quản trị viên; Học viên; Notification Service.

## Điều kiện đầu vào

- Quản trị viên đã được xác thực và được phép gửi thông báo.
- Học viên nhận thông báo tồn tại và có trạng thái `ACTIVE`.
- `bodyMarkdown` là Markdown hợp lệ và đã có lượt sử dụng media nếu nội dung nhúng media.

## UML luồng chạy

### `POST /api/notifications`

```mermaid
sequenceDiagram
    participant Admin
    participant API as Notification Service
    participant Student as Student Service
    participant DB as MySQL Notification

    Admin->>API: POST /api/notifications
    API->>API: Authorize + validate title/Markdown
    API->>Student: Verify recipient ACTIVE
    Student-->>API: Student exists
    alt Không hợp lệ/không tồn tại
        API-->>Admin: 400/403/404
    else Hợp lệ
        API->>DB: INSERT notification UNREAD
        DB-->>API: Created notification
        API-->>Admin: 201 notification
    end
```

### `GET /api/notifications/me`

```mermaid
sequenceDiagram
    participant Student
    participant API as Notification Service
    participant DB as MySQL Notification

    Student->>API: GET /api/notifications/me
    API->>API: Resolve authenticated student identity
    API->>DB: SELECT notifications by recipient_student_id
    DB-->>API: Inbox items
    API-->>Student: 200 inbox
```

### `PATCH /api/notifications/{id}/read`

```mermaid
sequenceDiagram
    participant Student
    participant API as Notification Service
    participant DB as MySQL Notification

    Student->>API: PATCH /api/notifications/{id}/read
    API->>DB: Read notification + recipient check
    DB-->>API: Notification hoặc rỗng
    alt Không phải người nhận hoặc không tồn tại
        API-->>Student: 403/404
    else Được phép
        API->>DB: UPDATE status=READ, read_at
        API-->>Student: 200 updated notification
    end
```

## Luồng gửi đơn

1. Quản trị viên gửi `POST /api/notifications` với `studentId`, `title` và `bodyMarkdown`.
2. Notification Service xác thực dữ liệu và xác nhận Học viên.
3. Notification Service tạo một bản ghi `notifications`:
   - `recipient_student_id` là Học viên nhận.
   - `source_type` là `SINGLE`.
   - `status` là `UNREAD`.
   - `notification_batch_id` là `null`.
4. API trả về mục hộp thư đến đã tạo.

## Luồng đọc hộp thư đến

1. Học viên gọi `GET /api/notifications/me`.
2. Notification Service lọc bằng danh tính đang được xác thực, không nhận `studentId` từ ứng dụng khách.
3. Học viên chọn một mục và gọi `PATCH /api/notifications/{id}/read`.
4. Notification Service kiểm tra `recipient_student_id` trùng Học viên hiện tại.
5. Notification Service chuyển `status` sang `READ` và lưu `read_at`.

## Trường hợp lỗi

- `403`: Quản trị viên không có quyền gửi hoặc Học viên cố đọc thông báo của người khác.
- `404`: Học viên hoặc thông báo không tồn tại.
- `409`: thông báo đã ở trạng thái `READ` nếu API chọn xử lý xung đột thay vì trả về thành công theo cách lũy đẳng.

## Dữ liệu thay đổi

- Cơ sở dữ liệu Notification Service: một bản ghi `notifications`.
- Không tạo `notification_batches`, `notification_batch_items` hoặc lượt chạy Scheduler.
