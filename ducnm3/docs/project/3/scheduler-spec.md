# Requirements — Scheduler

## Quy tắc chung

- `BR-SCHED-01`: Scheduler chỉ sở hữu job/run data; không truy vấn hoặc sửa trực
  tiếp database Media/Notification.
- `BR-SCHED-02`: Mỗi lượt chạy phải có idempotency key duy nhất trong job.
- `BR-SCHED-03`: Media Service quyết định điều kiện cleanup và là service duy
  nhất truy cập MinIO.

## F18 — Chạy dọn dẹp Media theo lịch

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Scheduler kích hoạt cleanup media không dùng và upload `PENDING` stale; Media Service thực hiện cleanup. |
| Preconditions | Job hợp lệ có lịch UTC và retention; contract cleanup nội bộ được duyệt. |
| Input / Output | Job schedule, idempotency key, tham số retention / job run và summary an toàn. |
| Main flow | Tính lượt chạy → tạo run idempotent → worker yêu cầu Media cleanup → lưu `SUCCEEDED`/`FAILED` cùng summary. |
| Alternative flow | Không có media cần dọn vẫn kết thúc `SUCCEEDED` với counter bằng 0; kích hoạt lại cùng idempotency key không tạo thêm job run. |
| Error cases | CRON/param sai; Media/MinIO lỗi; worker dừng giữa chừng; cùng idempotency key nhưng payload xung đột. |
| AC | Given job hợp lệ đến hạn, when kích hoạt, then có tối đa một run cho idempotency key; when cleanup được gọi lại, then object đã xóa không gây lỗi nghiêm trọng và Scheduler không truy cập Media database. |
| Cases | Happy: cleanup thành công. Boundary: không có media cần dọn, object đã xóa, kích hoạt lặp cùng payload. Negative: CRON sai, Media không sẵn sàng, idempotency key có payload xung đột. |

> [!NOTE]
> Function này là `Planned`: hiện tại chỉ có data boundary/health evidence.
> Thiết kế CRON, locking, retry, internal contract và worker execution thuộc
> Phase 4 trở đi.
