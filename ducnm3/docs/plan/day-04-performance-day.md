# NGÀY 4 — Ngày hiệu năng

Đây là ngày quan trọng nhất. Mục tiêu: hoàn thành toàn bộ benchmark trước và
sau khi tối ưu. Est là giờ làm việc thật, **không** chỉnh cho khớp 8 giờ/ngày.

Từ Ngày 3, mỗi hạng mục có Task, Est và Ticket. `Chưa tạo` = cấm push **code**.
Markdown tài liệu push thẳng `ducnm3`. Nhánh code: `feature/{mã backlog}`. Khi
user yêu cầu, agent tạo pull request vào `ducnm3`. Quy trình:
[DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md).

## Ước lượng thời gian

| Task | Est | Ticket |
| --- | --- | --- |
| CSV naive vs stream | 4 giờ | `Chưa tạo` |
| Chỉ mục + `EXPLAIN ANALYZE` | 5 giờ | `Chưa tạo` |
| Phân trang offset vs cursor | 4 giờ | `Chưa tạo` |
| Benchmark batch 3k/10k/100k | 4 giờ | `Chưa tạo` |

## Task dự kiến

### 1. CSV naive vs stream

- [ ] Triển khai `/courses/export-naive` và `/courses/export-stream`.
- [ ] Kiểm thử với 10k và 100k bản ghi.
- [ ] Ghi lại thời gian, bộ nhớ và kích thước tệp.
- Est: 4 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: hai endpoint, streaming vs buffer, đo 10k/100k và ghi
  `docs/benchmark`.

### 2. Chỉ mục và `EXPLAIN ANALYZE`

- [ ] Seed dữ liệu 100k khóa học; thử 1M nếu máy cho phép. Nếu seed 1M mất quá
  nhiều thời gian: giữ script riêng và tạo ít nhất một tập đủ lớn để thấy khác
  biệt.
- [ ] Truy vấn kiểm thử theo `status` + `created_at`.
- [ ] Đo trước khi tối ưu (không có chỉ mục) và sau khi tối ưu (chỉ mục kết hợp).
- [ ] Chạy `EXPLAIN ANALYZE`.
- Est: 5 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: seed 100k–1M tốn thời gian máy; thêm index, so sánh EXPLAIN và
  docs.

### 3. Phân trang offset vs cursor

- [ ] Kiểm thử OFFSET 0, 10k, 100k, 500k.
- [ ] Triển khai cursor `afterId`.
- [ ] So sánh thời gian trước/sau.
- Est: 4 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: bốn mốc OFFSET trên tập lớn + API cursor + bảng so sánh.

### 4. Benchmark batch 3k/10k/100k

- [ ] Kiểm thử xử lý theo lô với 3k, 10k, 100k.
- [ ] Ghi lại thời gian, bộ nhớ và thông lượng.
- Est: 4 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: ba quy mô, đo throughput/memory và ghi `docs/benchmark`. Phụ thuộc
  Worker batch đã chạy được.

## Tiêu chí hoàn thành Ngày 4

- [ ] CSV trước/sau tối ưu có số liệu.
- [ ] N+1 trước/sau tối ưu có số lượng truy vấn.
- [ ] Chỉ mục trước/sau tối ưu có EXPLAIN.
- [ ] Phân trang trước/sau tối ưu có số liệu thời gian.
- [ ] Có đánh giá hiệu năng xử lý theo lô.
- [ ] Lưu kết quả vào `docs/benchmark`.
