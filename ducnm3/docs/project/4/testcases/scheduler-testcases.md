# Testcase — Scheduler

F18 hiện là `Planned`; các testcase dưới đây ở trạng thái `Designed/Planned`.

| TC ID | Trace | Level | Setup / Test data | Action | Expected |
| --- | --- | --- | --- | --- | --- |
| TC-SCHED-F18-001 | F18 AC, BR-SCHED-01/03 | Integration | Job đến hạn; Media có stale candidate; broker/MinIO sẵn sàng | Scheduler tạo run và gửi cleanup | Một run `RUNNING`; command durable; Media owner xóa đúng candidate; terminal event đưa run về `SUCCEEDED`; Scheduler không truy cập Media DB/MinIO. |
| TC-SCHED-F18-002 | F18 alternative, BR-SCHED-02 | Integration | Không có candidate; kích hoạt lại cùng idempotency key | Chạy hai lần | Tối đa một run; kết thúc `SUCCEEDED`, counter 0; không có command/side effect trùng. |
| TC-SCHED-F18-003 | F18 negative | Unit + integration | CRON/cutoff sai, Media unavailable hoặc key trùng khác payload | Trigger/consume | Validation chặn trước write hoặc run `FAILED` traceable; safe error; không lộ storage key/exception. |
| TC-SCHED-F18-004 | F18 recovery | Integration | Worker dừng giữa page; terminal event redelivery | Resume/consume lại | Candidate đã terminal không xử lý lại; một terminal run; counter không cộng lặp. |

Integration test cần MySQL, RabbitMQ và MinIO Testcontainers cô lập; không dùng
stack Docker Compose của developer.

