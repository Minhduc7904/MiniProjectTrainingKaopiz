# Lấy tổng Course và Lesson

## Luồng chính

1. Admin gọi `GET /course/api/courses/summary` qua Gateway.
2. Course Service xác nhận actor ADMIN.
3. Repository đếm mọi row `courses` và `lessons` trong database Course sở hữu.
4. Service trả `200` envelope với hai tổng và `Cache-Control: no-store`.

Endpoint không có request body, không truy cập database service khác và không thay đổi dữ liệu.
