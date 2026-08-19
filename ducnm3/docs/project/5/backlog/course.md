# Course Service Tasks — Phase 5

## P5-05 — F01 Triển khai quản lý Khóa học

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 8 giờ |
| Ticket | `Chưa tạo` |
| Loại | Planned |
| Dependency | P5-02 |
| Baseline | F01, TC-COURSE-F01-001..003 |

**Phạm vi:** Triển khai `POST /api/courses` và `PATCH /api/courses/{courseId}`
theo Clean Architecture; PATCH typed partial update, `{}` là no-op, null chỉ
clear description và publish phải kiểm tra invariant.

**Code dự kiến:** endpoint/contracts trong `CourseService.Api`, feature
Create/Update trong `CourseService.Application`, domain rule trong
`CourseService.Domain`, repository trong `CourseService.Infrastructure`; thay
`UnitTest1.cs` bằng test có ý nghĩa và thêm component/integration test nếu cần.

**Hoàn thành khi:** `201 + Location` cho create; update/not-found/invalid
transition đúng contract; DB state được kiểm tra; test happy/boundary/negative
pass và API docs/Postman có runtime evidence.

## P5-06 — F02 Triển khai quản lý Bài học

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 6 giờ |
| Ticket | `Chưa tạo` |
| Loại | Planned |
| Dependency | P5-05 |
| Baseline | F02, TC-COURSE-F02-001..003 |

**Phạm vi:** Triển khai tạo và PATCH Lesson; validate Course parent, title,
display order, content và unique `(course_id, display_order)` trong transaction.

**Code dự kiến:** endpoint/contracts Lesson trong `CourseService.Api`, feature
Create/Update Lesson trong Application, repository EF và test projects Course.

**Hoàn thành khi:** create/update trả đúng envelope; parent không tồn tại và
duplicate order không tạo side effect; constraint/race được integration test;
testcase F02 và docs/Postman được đồng bộ.

## P5-07 — F03 Triển khai tra cứu Khóa học và tối ưu query

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 12 giờ |
| Ticket | `Chưa tạo` |
| Loại | Planned |
| Dependency | P5-05, P5-06 |
| Baseline | F03, TC-COURSE-F03-001..003, NFR-QUERY/INDEX/PLAN/PAGING/API |

**Phạm vi:** Triển khai list offset/cursor, detail và details có Lesson/Progress;
stable ordering, projection và query shape tránh N+1.

**Code dự kiến:** bốn endpoint/query feature trong Course Api/Application,
read repository EF, migration version mới nếu benchmark chứng minh cần index,
unit/component/integration test và performance script/evidence.

**Hoàn thành khi:** functional testcase pass; query count không tăng theo số
record; benchmark 10k/100k và index experiment 10k/100k/1M có raw metrics;
`EXPLAIN` trước/sau được lưu; offset/cursor được so sánh trên cùng dataset và
cấu hình.

## P5-08 — F04 Triển khai ghi danh Khóa học idempotent

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 8 giờ |
| Ticket | `Chưa tạo` |
| Loại | Planned |
| Dependency | P5-04, P5-05 |
| Baseline | F04, TC-COURSE-F04-001..003, Q4-02, Q4-03 |

**Phạm vi:** Triển khai enrollment theo current Student từ identity boundary;
không tin `studentId` trong payload; kiểm tra Course/Student và chống duplicate
bằng business key `(courseId, studentId)` cùng idempotency contract đã duyệt.

**Code dự kiến:** enrollment endpoint/contracts, Application command/handler,
Student typed HTTP client, repository EF và test Course.

**Hoàn thành khi:** request đầu tạo đúng Enrollment và `Location`; replay/race
không tạo row thứ hai; unauthorized/invalid course/student/duplicate behavior
đúng contract; unit/component/integration test xác minh persistent state.

## P5-09 — F05 Triển khai cập nhật tiến độ Bài học

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 6 giờ |
| Ticket | `Chưa tạo` |
| Loại | Planned |
| Dependency | P5-06, P5-08 |
| Baseline | F05, TC-COURSE-F05-001..003 |

**Phạm vi:** PUT full representation `progressPercent` cho current Student;
validate enrollment và Lesson thuộc Course; upsert idempotent trong một
transaction; `100` đặt `completedAt`, replay không đổi timestamp.

**Code dự kiến:** progress endpoint/contracts, Application feature, EF
repository và Course test projects.

**Hoàn thành khi:** range/boundary, ownership, not enrolled, replay và
concurrency được test; DB state/`completedAt` đúng; testcase F05 và docs/Postman
được đồng bộ.

## P5-10 — F06 Triển khai CSV streaming cho Khóa học

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 8 giờ |
| Ticket | `ERBUL26-3019` |
| Loại | Planned |
| Dependency | P5-07 |
| Baseline | F06, TC-COURSE-F06-001..003, NFR-CSV-01 |

**Phạm vi:** Triển khai `GET /api/courses/export` bằng streaming/chunking,
escape CSV đúng và không materialize toàn dataset/string trong memory.

**Code dự kiến:** export endpoint, export query/stream writer, repository
stream/page và test/performance evidence của Course; thêm migration chỉ khi
index được benchmark chứng minh.

**Hoàn thành khi:** CSV header/encoding/escaping/filter đúng; client cancel
được xử lý; benchmark 100k+ so sánh SELECT ALL/build string với streaming trên
cùng máy/dataset; lưu execution time, peak memory và raw output evidence.
