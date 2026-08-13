# `GET /health`

## Mục đích

Kiểm tra Scheduler Service đang chạy và cơ sở dữ liệu Scheduler do dịch vụ sở hữu chấp nhận một truy vấn nhẹ.

## Xác thực và phân quyền

- Xác thực: không bắt buộc.
- Vai trò/phạm vi: không có.
- Quy tắc sở hữu: không áp dụng.

## Yêu cầu

Không có tham số đường dẫn, tham số truy vấn, nội dung yêu cầu hoặc phân trang.

## Phản hồi thành công

```http
200 OK
```

```json
{
  "data": {
    "service": "scheduler-service",
    "status": "healthy",
    "database": {
      "status": "healthy"
    }
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Mã trạng thái HTTP

- `200`: Scheduler Service và `lms_scheduler_db` đều khả dụng.
- `503 DATABASE_UNAVAILABLE`: API đang chạy nhưng không thể truy vấn cơ sở dữ liệu.

## Điều kiện nghiệp vụ và tác động phụ

Chỉ thực thi `SELECT 1` trên `lms_scheduler_db`. Điểm cuối không tạo, nhận, phân tích hoặc thực thi tác vụ và không gọi dịch vụ khác.
