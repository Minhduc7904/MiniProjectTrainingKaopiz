# `GET /api/notification-batches/{batchId}/failed-items`

Business flow: [Xem lỗi người nhận của batch thông báo](../../../business-flows/notifications/get-notification-batch-failed-items.md).

## Mục đích

Liệt kê các mục người nhận thất bại trong một lô hàng loạt của Notification Service.

## Xác thực và phân quyền

- MVP hiện chưa có auth; khi auth được bổ sung, endpoint phải giới hạn quyền xem
  theo chính sách thông báo của tổ chức sở hữu.

## Yêu cầu

- `batchId`: tham số đường dẫn UUID bắt buộc.
- `cursor`: mã tiếp tục không trong suốt, không bắt buộc.
- `limit`: số nguyên không bắt buộc từ 1 đến 100; mặc định là 100.
- Không có nội dung yêu cầu.

## Phản hồi thành công

Thứ tự ổn định theo `notification_batch_items.id` tăng dần. Máy khách phải coi
`nextCursor` là giá trị không trong suốt và không tự tạo cursor.

```http
200 OK
```

```json
{
  "data": {
    "items": [
      {
        "studentId": "4691356d-12d2-44e1-a7d1-cad9959c4bf3",
        "retryCount": 1,
        "errorMessage": "Sender timeout"
      }
    ]
  },
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "cursor",
      "limit": 100,
      "nextCursor": "opaque-token",
      "hasNextPage": true
    }
  }
}
```

## Mã trạng thái HTTP

- `200`: đã trả về các mục thất bại.
- `400 VALIDATION_FAILED`: ID, `limit` hoặc `cursor` không hợp lệ.
- `401`: thiếu thông tin xác thực hoặc thông tin xác thực không hợp lệ.
- `403`: bên gọi không có quyền xem lô.
- `404 NOTIFICATION_BATCH_NOT_FOUND`: lô không tồn tại.
- `500 UNEXPECTED_ERROR`: phản hồi an toàn cho lỗi không mong đợi.

## Điều kiện nghiệp vụ và tác động phụ

GET an toàn, idempotent, `Cache-Control: no-store`. Endpoint đọc
`notification_batches` để xác nhận batch tồn tại, rồi đọc
`notification_batch_items` có `status = FAILED`. Không kích hoạt retry hoặc tạo
lượt chạy Worker. Trong khi dữ liệu còn thay đổi, item mới có thể xuất hiện sau
cursor hiện tại; UI chỉ đọc lỗi chi tiết khi batch đã ở trạng thái cuối.
