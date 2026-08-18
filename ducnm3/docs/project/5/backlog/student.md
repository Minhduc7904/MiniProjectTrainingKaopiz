# Student Service Tasks — Phase 5

## P5-03 — F07 Hoàn thiện tra cứu danh sách Học viên

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 4 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-02 |
| Baseline | F07, TC-STUDENT-F07-001..003, NFR-PAGING-01, NFR-API-01 |

**Phạm vi:** Audit `GetStudentsEndpoint`, `GetStudentsHandler`,
`EfStudentRepository` và endpoint cursor nội bộ; giữ stable ordering, validate
filter/sort/page và không làm thay đổi response contract hiện có.

**Code/test chính:** `backend/Services/Student/StudentService.Api/Endpoints/`,
`StudentService.Application/Features/Students/GetList/`,
`StudentService.Infrastructure/Persistence/EfStudentRepository.cs`, cùng
UnitTests, ComponentTests và IntegrationTests tương ứng.

**Hoàn thành khi:** functional testcase pass; benchmark 10k/100k ghi response
time và query count; offset lớn được đo và so sánh với cursor; `EXPLAIN` xác
nhận index/order; docs và raw metrics được lưu.

## P5-04 — F08 Hoàn thiện tra cứu chi tiết Học viên

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 3 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-02 |
| Baseline | F08, TC-STUDENT-F08-001..003 |

**Phạm vi:** Audit endpoint/handler/repository hiện có cho valid ID, malformed
ID, not found và data mapping; giữ endpoint read-only và không lộ field ngoài
contract.

**Code/test chính:** `StudentService.Api/Endpoints/StudentEndpoints.cs`,
`StudentService.Application/Features/Students/GetById/`,
`StudentService.Infrastructure/Persistence/EfStudentRepository.cs`, UnitTests
và component/integration test liên quan.

**Hoàn thành khi:** ba testcase F08 pass, error envelope đúng baseline, query
chỉ đọc Student DB và endpoint có runtime evidence trong `docs/tests/`.
