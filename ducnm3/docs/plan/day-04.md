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
| Hoàn thiện Phase 3 — Requirements | 8 giờ | `ERBUL26-2827` |
| Hoàn thiện Phase 4 — Basic Design + Testcase | 16 giờ | `ERBUL26-2834` |

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

### 4. Hoàn thiện Phase 3 — Requirements

- [x] Review scope Phase 0–2 và phân rã thành Function List.
- [x] Viết spec theo domain, Business Rules, Acceptance Criteria và các case
  happy/boundary/negative.
- [x] Xác định NFR, traceability và tiêu chí G2 — Requirements Approved.
- Est: 8 giờ.
- Ticket: `ERBUL26-2827`.
- Lý do est: tổng hợp capability và phần Planned của năm service, chuẩn hóa yêu
  cầu có thể kiểm thử mà không đi trước Phase 4 Design + Testcase.

### 5. Hoàn thiện Phase 4 — Basic Design + Testcase

- [x] Lập design baseline và ánh xạ F01–F18 tới architecture, API, database,
  messaging/worker hiện có hoặc phần `Planned`.
- [x] Thiết kế testcase theo domain cho happy/boundary/negative case và side
  effect cần xác minh.
- [x] Thiết kế testcase NFR cho Docker, batch, CSV, query, pagination,
  reliability, security, observability và error handling.
- [x] Hoàn thiện traceability từ Function/BR/AC/NFR tới design và testcase.
- [ ] Đóng Q&A critical và nhận Lead/TL/SQA sign-off G3.
- Est: 16 giờ.
- Ticket: `ERBUL26-2834`.
- Lý do est: audit và tái sử dụng design hiện có, bổ sung contract còn thiếu,
  chuẩn hóa testcase cho 18 function/15 NFR và chuẩn bị cổng G3.

## Tiêu chí hoàn thành Ngày 4

- [ ] Mỗi service có data model và ERD riêng.
- [ ] Ownership database, ràng buộc và index được mô tả rõ.
- [ ] Liên kết từ tài liệu architecture đến database còn hợp lệ.
- [x] Mỗi service và BuildingBlocks có tài liệu tổng quan cùng tài liệu chi tiết
  theo layer/runtime đang tồn tại.
- [x] Phase 0–2 có project initiation, kickoff, planning, registers và
  traceability ở mức lead.
- [x] Phase 3 có Function List, functional specification, NFR và traceability
  cho các phạm vi đã chốt.
- [x] Phase 4 có Basic Design baseline, testcase catalog và traceability draft;
  G3 chờ review/sign-off.
