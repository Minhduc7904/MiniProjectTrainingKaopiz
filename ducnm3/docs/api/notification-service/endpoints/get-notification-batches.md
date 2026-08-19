# `GET /api/notification-batches`

## Mục đích

Liệt kê Notification Batch cho trang quản lý, sắp xếp cố định `created_at DESC, id DESC` và trả tổng thời gian chạy.

## Yêu cầu

- `status`: tùy chọn, một trạng thái batch hợp lệ.
- `page`: mặc định `1`, tối thiểu `1`.
- `pageSize`: mặc định `20`, từ `1` đến `100`.

## Phản hồi

`200 OK`, `data` là mảng cùng schema với GET detail. `meta.pagination` có `type=offset`, `page`, `pageSize`, `totalItems`, `totalPages`. Response đặt `Cache-Control: no-store`.

`400 VALIDATION_FAILED` được trả khi status hoặc pagination không hợp lệ. Endpoint chỉ đọc database. FE poll mỗi 5 giây khi trang hiện có batch chưa terminal và tự cập nhật duration giữa hai lần poll.
