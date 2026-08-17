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
| Hoàn thiện kiến trúc | 8 giờ | `ERBUL26-2724` |
| Hoàn thiện Phase 0–2 | 8 giờ | `ERBUL26-2726` |

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

### 2. Hoàn thiện kiến trúc

- [x] Tái cấu trúc tài liệu kiến trúc theo Backend và Frontend.
- [x] Bổ sung `architecture.md` tổng quan và tài liệu chi tiết theo layer cho
  từng service có source tương ứng.
- [x] Chuẩn hóa BuildingBlocks theo cấu trúc tổng quan và `details/`.
- Est: 8 giờ.
- Ticket: `ERBUL26-2724`.
- Lý do est: rà soát dependency, runtime boundary, database/integration và test
  boundary của năm service cùng BuildingBlocks; viết và kiểm tra liên kết tài liệu.

### 3. Hoàn thiện Phase 0–2

- [x] Rà soát source, Docker Compose và tài liệu hiện có.
- [x] Lập Project Initiation, Kickoff và Planning ở mức high-level.
- [x] Ghi assumptions, risks, traceability và tiêu chí hoàn thành Phase 0–2.
- Est: 8 giờ.
- Ticket: `ERBUL26-2726`.
- Lý do est: tổng hợp scope/constraint/dependency thực tế, chuẩn hóa planning
  artifacts và phân biệt capability đã có với mục tiêu Phase sau.

## Tiêu chí hoàn thành Ngày 4

- [ ] Mỗi service có data model và ERD riêng.
- [ ] Ownership database, ràng buộc và index được mô tả rõ.
- [ ] Liên kết từ tài liệu architecture đến database còn hợp lệ.
- [x] Mỗi service và BuildingBlocks có tài liệu tổng quan cùng tài liệu chi tiết
  theo layer/runtime đang tồn tại.
- [x] Phase 0–2 có project initiation, kickoff, planning, registers và
  traceability ở mức lead.
