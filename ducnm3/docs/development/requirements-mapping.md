# Đối chiếu yêu cầu

| Yêu cầu | Nội dung minh họa trong project |
|---|---|
| Docker | Gateway, 4 service nghiệp vụ, Scheduler Service, MySQL và MinIO |
| MinIO | Media Service tải lên và truyền phát media |
| Job theo lô | Gửi thông báo đến 3k/10k/100k học viên |
| Thử lại | Mô phỏng bộ gửi thất bại với một lần thử lại |
| Hiệu năng xử lý theo lô | Đo thời gian và bộ nhớ cho 3k/10k/100k |
| Xuất CSV | Xuất hơn 100k khóa học |
| Hiệu năng CSV | So sánh tải toàn bộ và streaming |
| N+1 | Truy vấn khóa học, bài học và tiến độ |
| Chỉ mục và kế hoạch truy vấn | So sánh `EXPLAIN ANALYZE` trước và sau khi tạo chỉ mục |
| Phân trang | So sánh phân trang offset và cursor |
| Đánh giá hiệu năng API | Đánh giá các endpoint trước và sau khi tối ưu |
| Xử lý lỗi | Hợp đồng chuẩn cho lỗi 400, 404, 409 và 500 |
