# `GET /api/notification-batches/{batchId}`

## Mục đích

Trả về các bộ đếm gửi và trạng thái của Notification Service cho một lô hàng loạt.

## Xác thực và phân quyền

- Xác thực: bắt buộc.
- Vai trò/phạm vi: quản trị viên thông báo.
- Quy tắc sở hữu: quyền truy cập tuân theo chính sách thông báo của tổ chức sở hữu.

## Yêu cầu

- `batchId`: tham số đường dẫn UUID bắt buộc.
- Không có tham số truy vấn hoặc nội dung yêu cầu.

## Phản hồi thành công

```http
200 OK
```

```json
{
  "data": {
    "id": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "PROCESSING",
    "totalCount": 3000,
    "processedCount": 1500,
    "successCount": 1490,
    "failedCount": 10
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Mã trạng thái HTTP

- `200`: đã tìm thấy lô.
- `400 VALIDATION_FAILED`: `batchId` không phải UUID.
- `401`: thiếu thông tin xác thực hoặc thông tin xác thực không hợp lệ.
- `403`: bên gọi không có quyền xem lô.
- `404 NOTIFICATION_BATCH_NOT_FOUND`: lô không tồn tại.
- `500 UNEXPECTED_ERROR`: phản hồi an toàn cho lỗi không mong đợi.

## Điều kiện nghiệp vụ và tác động phụ

Chỉ đọc `notification_batches` và không thay đổi trạng thái. Các bộ đếm mô tả việc gửi theo nghiệp vụ Notification, không phải lịch sử chạy Scheduler chung.
