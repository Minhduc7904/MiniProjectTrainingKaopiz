# Dự án thu gọn vi dịch vụ LMS — Kế hoạch triển khai 5 ngày

## 1. Mục tiêu dự án

Xây dựng một **LMS thu gọn** theo kiến trúc **Vi dịch vụ + Kiến trúc sạch**, đủ nhỏ để hoàn thành và trình diễn trong **5 ngày**, nhưng vẫn bao quát được các bài toán phía máy chủ mà trưởng nhóm yêu cầu:

- Docker / Docker Compose
- MySQL
- MinIO / Lưu trữ đối tượng
- Tác vụ theo lô
- Thử lại + Theo dõi lỗi + Tính lũy đẳng
- Hiệu năng xử lý theo lô
- Xuất CSV trên 100 nghìn bản ghi
- Truyền luồng / Phân đoạn CSV
- Truy vấn N+1
- Chỉ mục cơ sở dữ liệu
- `EXPLAIN` / `EXPLAIN ANALYZE`
- Phân trang theo độ lệch so với phân trang bằng con trỏ
- Đo hiệu năng API
- Xử lý lỗi
- UML / Sơ đồ kiến trúc
- Có số liệu **Trước / Sau tối ưu hóa**

> Mục tiêu của dự án thu gọn không phải làm một LMS đầy đủ tính năng.
> Mục tiêu là tạo đủ "đất diễn" để chứng minh hiểu kiến trúc, cơ sở dữ liệu, hiệu năng, xử lý theo lô và độ tin cậy.

---
# 2. Phạm vi chức năng

## 2.1. Tác nhân

### Quản trị viên

- Tạo/sửa/xóa khóa học.
- Tạo bài học.
- Tải lên và quản lý nhiều loại nội dung đa phương tiện: ảnh thu nhỏ, hình ảnh, video, âm thanh, tài liệu.
- Xem danh sách khóa học.
- Xuất khóa học ra CSV.
- Gửi thông báo cho một học viên.
- Gửi thông báo hàng loạt cho học viên.
- Xem trạng thái thông báo hàng loạt.
- Xem các mục gửi thất bại.

### Học viên

- Xem khóa học đã ghi danh.
- Xem bài học.
- Cập nhật tiến độ bài học.
- Xem thông báo trong hộp thư đến của chính mình và đánh dấu đã đọc.

---
# 3. Những chức năng KHÔNG làm trong 5 ngày

Để tránh phạm vi tăng mất kiểm soát:

- Thanh toán
- Thi thời gian thực
- Trò chuyện
- Phân quyền chi tiết
- Token làm mới phức tạp
- Trình chỉnh sửa mẫu email
- Chuyển mã video
- Công cụ tìm kiếm Elasticsearch
- Kafka/RabbitMQ
- Kubernetes
- Theo dõi phân tán phức tạp
- CQRS/Lưu nguồn sự kiện đầy đủ

Có thể dùng xác thực JWT đơn giản nếu cần trình diễn.

---
