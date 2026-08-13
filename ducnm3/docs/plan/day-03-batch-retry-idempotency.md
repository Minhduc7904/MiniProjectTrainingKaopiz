# NGÀY 3 — Xử lý theo lô + Thử lại + Tính idempotent

## Mục tiêu

Demo xử lý nền rõ ràng.

### Nhiệm vụ

#### Seed dữ liệu học viên

Tạo:

```text
3k
10k
100k
```

học viên.

#### Thông báo theo lô

```http
POST /notification-batches
```

Luồng:

```text
Tạo lô
→ Trả về 202
→ Worker xử lý
```

#### Thông báo đơn lẻ và hộp thư đến

```http
POST  /notifications
GET   /notifications/me
PATCH /notifications/{id}/read
```

- Gửi đơn lẻ tạo một mục trong hộp thư đến cho đúng một học viên.
- Học viên chỉ xem và đánh dấu đã đọc thông báo của chính mình.
- Worker xử lý theo lô cũng tạo một mục trong hộp thư đến cho từng người nhận.
- Nội dung thông báo lưu Markdown và đăng ký media nhúng/tệp đính kèm qua Media Service.

#### Chia lô

```text
batchSize = 500
```

#### Thử lại

```text
thử lại 1 lần
```

#### Lỗi

Lưu:

```text
status
retry_count
error_message
```

#### Tính idempotent

```text
UNIQUE(batch_id, student_id)
```

### Tiêu chí hoàn thành Ngày 3

- [ ] API trả về 202.
- [ ] Lần chạy Scheduler gọi trình xử lý thông báo theo lô.
- [ ] Mỗi lô gồm 500 mục.
- [ ] Cơ chế thử lại hoạt động.
- [ ] Mục thất bại được lưu.
- [ ] Tiến độ lô và lịch sử chạy Scheduler xem được ở đúng service.
- [ ] Khởi động lại không tạo trùng mục đã thành công.
- [ ] Gửi đơn lẻ hoạt động và học viên đọc/đánh dấu đã đọc thông báo của mình.
- [ ] Xử lý theo lô không tạo thông báo trùng khi worker thử lại hoặc khởi động lại.
- [ ] Markdown của thông báo hiển thị media thông qua URL của Media Service.

---
