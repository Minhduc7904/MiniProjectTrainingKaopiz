# Xem lỗi người nhận của batch thông báo

API contract: [GET failed items](../../api/notification-service/endpoints/get-notification-batch-failed-items.md).

## Mục đích

Quản trị viên đọc các người nhận đã `FAILED` để biết lỗi cuối cùng sau batch
gửi thông báo hàng loạt.

## Tác nhân và điều kiện đầu vào

- Quản trị viên có `batchId` UUID hợp lệ.
- `limit` từ 1 đến 100, mặc định 100; `cursor` là token do response trước trả về.

## Luồng chính

1. Client gửi `GET /api/notification-batches/{batchId}/failed-items` không body.
2. Notification API kiểm tra `batchId`, `limit`, `cursor` và batch tồn tại.
3. Repository đọc item `FAILED`, sắp xếp tăng dần theo `id`, lấy `limit + 1`.
4. API trả `200` envelope với `studentId`, `retryCount`, `errorMessage` và
   cursor pagination.
5. Nếu `hasNextPage=true`, client gửi `nextCursor` để đọc trang tiếp theo.

## Trường hợp rỗng và lỗi

- Batch không có người nhận lỗi: `200` với `items: []`.
- `batchId`, `limit` hoặc `cursor` không hợp lệ: `400 VALIDATION_FAILED`.
- Không có batch: `404 NOTIFICATION_BATCH_NOT_FOUND`.

## Dữ liệu thay đổi

Không có; endpoint chỉ đọc `notification_batches` và `notification_batch_items`.
