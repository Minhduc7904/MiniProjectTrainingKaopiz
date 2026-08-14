# NGÀY 5 — Xử lý lỗi + Kiểm thử + Bản trình chiếu + Demo

Est là giờ làm việc thật, **không** chỉnh cho khớp 8 giờ/ngày.

Từ Ngày 3, mỗi hạng mục có Task, Est và Ticket. `Chưa tạo` = cấm push **code**.
Markdown tài liệu push thẳng `ducnm3`. Nhánh code: `feature/{mã backlog}`. Khi
user yêu cầu, agent tạo pull request vào `ducnm3`. Quy trình:
[DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md).

## Ước lượng thời gian

| Task | Est | Ticket |
| --- | --- | --- |
| Chuẩn hóa lỗi API + CorrelationId/log batch | 4 giờ | `Chưa tạo` |
| Unit test tối thiểu | 3 giờ | `Chưa tạo` |
| Slide kiến trúc/UML/benchmark | 6 giờ | `Chưa tạo` |
| Kịch bản demo + diễn tập + README chạy | 3 giờ | `Chưa tạo` |

## Task dự kiến

### 1. Chuẩn hóa lỗi API và log

- [ ] Xử lý lỗi API: kiểm tra hợp lệ, không tìm thấy, xung đột, lỗi cơ sở dữ
  liệu / lỗi chưa được xử lý.
- [ ] Ghi log: CorrelationId, thời gian xử lý yêu cầu, nhật ký xử lý theo lô.
- Est: 4 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: rà soát mọi service, thống nhất envelope 400/404/409/500 và log
  batch.

### 2. Unit test tối thiểu

- [ ] Các kiểm thử tối thiểu: `CreateCourse`, thử lại thông báo, tính
  idempotent của tác vụ, kiểm tra hợp lệ.
- [ ] Không cố đạt độ bao phủ cực cao trong dự án nhỏ.
- Est: 3 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: bốn nhóm case, không chase coverage.

### 3. Slide kiến trúc, UML và benchmark

- [ ] Chuẩn bị bản trình chiếu: kiến trúc, UML, Docker, MinIO, xử lý theo lô,
  thử lại, CSV, N+1, chỉ mục, phân trang, đánh giá hiệu năng, kết luận.
- [ ] UML hoàn chỉnh.
- [ ] Bảng đánh giá hiệu năng hoàn chỉnh.
- Est: 6 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: nhiều slide + UML + chép số liệu benchmark.

### 4. Kịch bản demo, diễn tập và README chạy

- [ ] Viết kịch bản demo hoàn chỉnh.
- [ ] Chạy đúng tập lệnh ít nhất 1 lần trước demo.
- [ ] Docker Compose chạy ổn; tập lệnh seed chạy được.
- [ ] README có hướng dẫn chạy.
- [ ] Có ảnh chụp màn hình đánh giá hiệu năng dự phòng nếu demo trực tiếp gặp
  lỗi.
- Est: 3 giờ.
- Ticket: `Chưa tạo`.
- Lý do est: viết kịch bản, chạy một lượt thật, chụp màn hình dự phòng.

## Tiêu chí hoàn thành Ngày 5

- [ ] Bản trình chiếu hoàn chỉnh.
- [ ] UML hoàn chỉnh.
- [ ] Bảng đánh giá hiệu năng hoàn chỉnh.
- [ ] Kịch bản demo hoàn chỉnh.
- [ ] Docker Compose chạy ổn.
- [ ] Tập lệnh seed chạy được.
- [ ] README có hướng dẫn chạy.
- [ ] Có ảnh chụp màn hình đánh giá hiệu năng dự phòng nếu demo trực tiếp gặp lỗi.
