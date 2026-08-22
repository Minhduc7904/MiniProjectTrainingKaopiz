# Lấy tổng Media

## Luồng chính

1. Admin gọi `GET /media/api/media/summary` qua Gateway.
2. Media Service xác nhận actor ADMIN.
3. Repository đếm mọi row `media_objects` bằng query chỉ đọc.
4. Service trả `200` envelope với `totalMedia` và `Cache-Control: no-store`.

Endpoint không gọi MinIO, không tạo job, không có request body và không thay đổi dữ liệu.
