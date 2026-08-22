# Lấy tổng Student

## Luồng chính

1. Admin gửi `GET /student/api/students/summary` qua Gateway.
2. Student Service kiểm tra actor ADMIN.
3. Repository đếm mọi row `students` bằng query chỉ đọc.
4. Service trả envelope `200` với `totalStudents` và `Cache-Control: no-store`.

Request không có body, không thay đổi dữ liệu. Actor không hợp lệ trả `400` hoặc `403`.
