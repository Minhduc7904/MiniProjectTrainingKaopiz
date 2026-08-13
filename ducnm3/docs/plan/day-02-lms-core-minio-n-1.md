# NGÀY 2 — LMS cốt lõi + MinIO + N+1

## Mục tiêu

Có phần cốt lõi của LMS đủ để demo tối ưu truy vấn.

### Nhiệm vụ

#### Khóa học

- Tạo khóa học.
- Liệt kê khóa học.
- Tạo bài học.
- Ghi danh.
- Tiến độ học tập.

#### MinIO

- Media Service tạo `media_objects` và `media_usages` để lưu metadata, loại media, và vị trí sử dụng.
- Tải thumbnail/tài liệu lên qua Media Service; Course Service không gọi MinIO.
- Course Service lưu Markdown và gọi Media Service để liên kết thumbnail, nội
  dung nhúng hoặc tệp đính kèm.
- Tải xuống/lấy URL ký trước qua Media Service.

#### N+1

Tạo:

```text
GET /courses/details-naive
GET /courses/details-optimized
```

Cách đơn giản:

```text
N+1
```

Đã tối ưu:

```text
Phép chiếu / Phép nối
```

Bật ghi log SQL.

### Dữ liệu seed

```text
100 khóa học
20 bài học/khóa học
các bản ghi tiến độ
```

### Tiêu chí hoàn thành Ngày 2

- [ ] CRUD khóa học hoạt động.
- [ ] Bài học hoạt động.
- [ ] Tiến độ học tập hoạt động.
- [ ] Tải lên/tải xuống MinIO hoạt động.
- [ ] Khóa học hoặc bài học dùng được nhiều loại media qua Media Service.
- [ ] Có điểm cuối N+1.
- [ ] Có điểm cuối đã tối ưu.
- [ ] Có log SQL để so sánh số lượng truy vấn.

---
