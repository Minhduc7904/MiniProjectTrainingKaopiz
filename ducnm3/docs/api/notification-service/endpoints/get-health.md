# `GET /health`

## Mục đích

Kiểm tra Notification Service kết nối được database sở hữu và RabbitMQ.

## Xác thực và phân quyền

- Xác thực: không bắt buộc.
- Vai trò/phạm vi: không có.
- Quy tắc sở hữu: không áp dụng.

## Yêu cầu

Không có tham số đường dẫn, tham số truy vấn hoặc nội dung yêu cầu. Không áp dụng phân trang.

## Phản hồi thành công

```http
200 OK
```

```json
{
  "data": {
    "service": "notification-service",
    "status": "healthy",
    "database": {
      "status": "healthy"
    },
    "messaging": {
      "status": "healthy"
    }
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Mã trạng thái HTTP

- `200`: Notification Service, `lms_notification_db` và RabbitMQ đều khả dụng.
- `503 DATABASE_UNAVAILABLE`: Notification Service đang chạy nhưng không thể truy vấn cơ sở dữ liệu của mình.
- `503 DEPENDENCY_UNAVAILABLE`: MassTransit bus chưa kết nối RabbitMQ.

## Điều kiện nghiệp vụ và tác động phụ

Endpoint thực thi `SELECT 1` và đọc MassTransit health report. Endpoint không
ghi database hoặc publish message.
