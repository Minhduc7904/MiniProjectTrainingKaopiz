# NGÀY 4 — Hoàn thiện Database

Mục tiêu: hoàn thiện tài liệu data model và ERD theo ownership của từng service.
Est là giờ làm việc thật, **không** chỉnh cho khớp 8 giờ/ngày.

Từ Ngày 3, mỗi hạng mục có Task, Est và Ticket. `Chưa tạo` = cấm push **code**.
Markdown tài liệu push thẳng `ducnm3`. Nhánh code: `feature/{mã backlog}`. Khi
user yêu cầu, agent tạo pull request vào `ducnm3`. Quy trình:
[DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md).

## Ước lượng thời gian

| Task | Est | Ticket |
| --- | --- | --- |
| Hoàn thiện Database | 8 giờ | `ERBUL26-2722` |

## Task dự kiến

### 1. Hoàn thiện Database

- [ ] Tách data model theo ownership của Course, Student, Media, Notification
  và Scheduler Service.
- [ ] Bổ sung ERD và quy tắc ràng buộc/index cho từng service.
- [ ] Chuẩn hóa tham chiếu tài liệu architecture và database.
- Est: 8 giờ.
- Ticket: `ERBUL26-2722`.
- Lý do est: rà soát năm database service, tách data model, tạo ERD và kiểm tra
  các liên kết tài liệu.

## Tiêu chí hoàn thành Ngày 4

- [ ] Mỗi service có data model và ERD riêng.
- [ ] Ownership database, ràng buộc và index được mô tả rõ.
- [ ] Liên kết từ tài liệu architecture đến database còn hợp lệ.
