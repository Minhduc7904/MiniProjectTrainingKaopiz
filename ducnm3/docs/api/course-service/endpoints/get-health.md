# `GET /health`

## Mục đích

Kiểm tra Course Service đang chạy và cơ sở dữ liệu Course do dịch vụ sở hữu chấp nhận một truy vấn nhẹ.

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
    "service": "course-service",
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

- `200`: Course Service và `lms_course_db` đều khả dụng.
- `503 DATABASE_UNAVAILABLE`: Course Service đang chạy nhưng không thể truy vấn cơ sở dữ liệu của mình.

## Điều kiện nghiệp vụ và tác động phụ

Điểm cuối chỉ thực thi `SELECT 1` trên cơ sở dữ liệu riêng của Course Service. Điểm cuối không ghi dữ liệu hoặc gọi dịch vụ khác.
