# Định dạng phản hồi dùng chung

Tất cả điểm cuối JSON đều sử dụng cấu trúc bao này. Xuất CSV, truyền tệp theo luồng và chuyển hướng đến URL phương tiện được ký trước là các ngoại lệ không dùng JSON đã được quy định rõ.

## Tài nguyên đơn hoặc lệnh thành công

```json
{
  "data": {
    "id": "course-uuid",
    "name": "Backend Fundamentals"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

- `data`: tài nguyên được yêu cầu, kết quả lệnh hoặc `null` khi chủ đích trả về kết quả thành công rỗng.
- `meta.traceId`: ID tương quan dùng cho nhật ký và hỗ trợ; luôn có trong phản hồi ở môi trường sản xuất.

## Phân trang theo con trỏ

```json
{
  "data": [
    {
      "id": "notification-uuid",
      "title": "Course update"
    }
  ],
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "cursor",
      "limit": 20,
      "nextCursor": "opaque-token",
      "hasNextPage": true
    }
  }
}
```

- `nextCursor` là `null` khi không còn trang tiếp theo.
- Con trỏ là giá trị không trong suốt, không được làm lộ ID cơ sở dữ liệu hoặc trường sắp xếp nội bộ.
- Điểm cuối xác định và ghi tài liệu cho một thứ tự sắp xếp ổn định.

## Phân trang theo độ lệch

```json
{
  "data": [
    {
      "id": "course-uuid",
      "name": "Backend Fundamentals"
    }
  ],
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "offset",
      "page": 1,
      "pageSize": 20,
      "totalItems": 101,
      "totalPages": 6
    }
  }
}
```

- `page` bắt đầu từ 1.
- `pageSize` là kích thước được yêu cầu và đã kiểm tra hợp lệ.
- `totalItems` và `totalPages` chỉ bắt buộc với phân trang theo độ lệch.

## Lỗi

```json
{
  "error": {
    "code": "COURSE_NOT_FOUND",
    "message": "Course not found",
    "details": []
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

- `error.code`: mã lỗi ổn định của ứng dụng.
- `error.message`: thông báo an toàn dành cho bên sử dụng API.
- `error.details`: mảng chi tiết kiểm tra hợp lệ không bắt buộc; bỏ qua khi không có chi tiết phù hợp.
- Tuyệt đối không làm lộ dấu vết ngăn xếp, SQL, thông tin xác thực, tên vùng lưu trữ MinIO hoặc khóa đối tượng.

## Tình trạng dịch vụ

Mỗi dịch vụ cung cấp `GET /health`. Điểm cuối này không có nội dung yêu cầu, tham số truy vấn hoặc phân trang.

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

- `200`: dịch vụ HTTP đang chạy và cơ sở dữ liệu do dịch vụ sở hữu chấp nhận `SELECT 1`.
- `503 DATABASE_UNAVAILABLE`: dịch vụ HTTP đang chạy nhưng không thể kết nối đến cơ sở dữ liệu do dịch vụ sở hữu.
- `503 SERVICE_UNAVAILABLE`: API Gateway không thể kết nối đến dịch vụ hạ nguồn.

Media Service còn bao gồm `data.storage.status` khi thành công vì MinIO là
thành phần phụ thuộc vận hành do dịch vụ sở hữu. Các lỗi bổ sung gồm:

- `503 STORAGE_UNAVAILABLE`: chỉ thành phần phụ thuộc MinIO không khả dụng.
- `503 DEPENDENCY_UNAVAILABLE`: cả cơ sở dữ liệu của Media Service và MinIO đều không khả dụng.
