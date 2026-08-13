# NGÀY 4 — Ngày hiệu năng

Đây là ngày quan trọng nhất.

## Mục tiêu

Hoàn thành toàn bộ benchmark trước và sau khi tối ưu.

---

## Nhiệm vụ 1 — CSV

Triển khai:

```text
/courses/export-naive
/courses/export-stream
```

Kiểm thử:

```text
10k
100k
```

Ghi lại:

```text
Thời gian
Bộ nhớ
Kích thước tệp
```

---

## Nhiệm vụ 2 — Chỉ mục

Dữ liệu seed:

```text
100k
1M khóa học
```

Nếu seed 1M bản ghi mất quá nhiều thời gian:

- Giữ script riêng.
- Tạo ít nhất một tập dữ liệu đủ lớn để thấy khác biệt.
- Nhưng mục tiêu cuối vẫn là thử 1M nếu máy cho phép.

Truy vấn kiểm thử:

```text
status + created_at
```

Trước khi tối ưu:

```text
Không có chỉ mục
```

Sau khi tối ưu:

```text
Chỉ mục kết hợp
```

Chạy:

```sql
EXPLAIN ANALYZE
```

---

## Nhiệm vụ 3 — Phân trang

Kiểm thử:

```text
OFFSET 0
OFFSET 10k
OFFSET 100k
OFFSET 500k
```

Triển khai cursor:

```text
afterId
```

So sánh.

---

## Nhiệm vụ 4 — Đánh giá hiệu năng xử lý theo lô

Kiểm thử:

```text
3k
10k
100k
```

Ghi lại:

```text
Thời gian
Bộ nhớ
Thông lượng
```

---

## Tiêu chí hoàn thành Ngày 4

- [ ] CSV trước/sau tối ưu có số liệu.
- [ ] N+1 trước/sau tối ưu có số lượng truy vấn.
- [ ] Chỉ mục trước/sau tối ưu có EXPLAIN.
- [ ] Phân trang trước/sau tối ưu có số liệu thời gian.
- [ ] Có đánh giá hiệu năng xử lý theo lô.
- [ ] Lưu kết quả vào `docs/benchmark`.

---
