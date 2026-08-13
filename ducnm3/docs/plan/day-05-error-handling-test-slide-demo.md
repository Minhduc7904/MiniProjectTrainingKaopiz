# NGÀY 5 — Xử lý lỗi + Kiểm thử + Bản trình chiếu + Demo

## Buổi sáng

### Xử lý lỗi API

- Kiểm tra hợp lệ.
- Không tìm thấy.
- Xung đột.
- Lỗi cơ sở dữ liệu/lỗi chưa được xử lý.

### Ghi log

- CorrelationId.
- Thời gian xử lý yêu cầu.
- Nhật ký xử lý theo lô.

### Kiểm thử đơn vị

Các kiểm thử tối thiểu:

```text
CreateCourse
Thử lại thông báo
Tính idempotent của tác vụ
Kiểm tra hợp lệ
```

Không cố đạt độ bao phủ cực cao trong dự án nhỏ.

---

## Buổi chiều

### Bản trình chiếu

Chuẩn bị:

```text
Kiến trúc
UML
Docker
MinIO
Xử lý theo lô
Thử lại
CSV
N+1
Chỉ mục
Phân trang
Đánh giá hiệu năng
Kết luận
```

### Diễn tập demo

Chạy đúng tập lệnh ít nhất 1 lần trước demo.

### Tiêu chí hoàn thành Ngày 5

- [ ] Bản trình chiếu hoàn chỉnh.
- [ ] UML hoàn chỉnh.
- [ ] Bảng đánh giá hiệu năng hoàn chỉnh.
- [ ] Kịch bản demo hoàn chỉnh.
- [ ] Docker Compose chạy ổn.
- [ ] Tập lệnh seed chạy được.
- [ ] README có hướng dẫn chạy.
- [ ] Có ảnh chụp màn hình đánh giá hiệu năng dự phòng nếu demo trực tiếp gặp lỗi.

---
