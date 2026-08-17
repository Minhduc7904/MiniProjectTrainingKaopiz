# Testcase — Course

## Cách đọc

Tất cả testcase trong file này ở trạng thái `Designed/Planned` vì Function List
chưa có runtime evidence cho F01–F06. `Action` mô tả thao tác qua contract Phase
4; `Expected` phải được kiểm tra cả response và database khi implement.

| TC ID | Trace | Level | Setup / Test data | Action | Expected |
| --- | --- | --- | --- | --- | --- |
| TC-COURSE-F01-001 | F01, BR-COURSE-01 | Component + integration | Admin hợp lệ; name/Markdown hợp lệ | Tạo Course | `201`; đúng một `courses` row `DRAFT`; `Location`/envelope đúng contract. |
| TC-COURSE-F01-002 | F01 AC, BR-COURSE-02 | Unit + integration | Course `DRAFT` thiếu điều kiện publish | PATCH status `PUBLISHED` | Bị từ chối bằng business error; Course vẫn `DRAFT`; không có side effect phụ. |
| TC-COURSE-F01-003 | F01 boundary | Component | Name/Markdown tại min/max và ngay ngoài giới hạn | Tạo/cập nhật Course | Giá trị biên hợp lệ được nhận; ngoài giới hạn trả `400` trước DB write. |
| TC-COURSE-F02-001 | F02, BR-COURSE-03 | Integration | Course tồn tại; order chưa dùng | Tạo Lesson | `201`; Lesson thuộc đúng Course và order. |
| TC-COURSE-F02-002 | F02 AC | Integration | Hai request dùng cùng `(courseId, displayOrder)` | Gửi đồng thời | Chỉ một row thắng; request còn lại `409`; không có order trùng. |
| TC-COURSE-F02-003 | F02 negative | Component | courseId sai hoặc actor không có quyền | Tạo/cập nhật Lesson | `404`/`403` an toàn; không ghi `lessons`. |
| TC-COURSE-F03-001 | F03 AC | Component + integration | Course PUBLISHED/DRAFT, Lessons, Progress | List/detail bằng actor đủ quyền | Dữ liệu và visibility đúng; stable ordering; không trả Course khác. |
| TC-COURSE-F03-002 | F03 alternative | Component | Filter hợp lệ không khớp record; cursor cuối | List Course | `200`, list rỗng và pagination metadata nhất quán. |
| TC-COURSE-F03-003 | F03 negative | Component | Filter/cursor malformed | Query | `400`; không chạy query không giới hạn; safe error envelope. |
| TC-COURSE-F04-001 | F04, BR-COURSE-02 | Integration | Student ACTIVE; Course PUBLISHED; chưa enrollment | Enroll | Tạo đúng một enrollment cho `(course, student)` và trả logical resource. |
| TC-COURSE-F04-002 | F04 alternative | Integration | Enrollment đã tồn tại; cùng actor/course/idempotency input | Gửi lại | Trả cùng logical Enrollment; tổng row vẫn là một; không lặp side effect. |
| TC-COURSE-F04-003 | F04 negative | Component + integration | Student INACTIVE hoặc Course DRAFT | Enroll | `403`/`409` theo contract; không ghi enrollment. |
| TC-COURSE-F05-001 | F05 AC | Integration | Student đã enrollment; Lesson thuộc Course | PUT progress `100` | Một progress row, percent 100 và `completed_at` được đặt. |
| TC-COURSE-F05-002 | F05 alternative/boundary | Integration | Progress đã 100 | PUT lại 100; thử 0 | Không tạo row/thời điểm hoàn thành mới; 0 và 100 được xử lý đúng policy. |
| TC-COURSE-F05-003 | F05 negative | Component | Percent `-1`/`100.01`, lesson ngoài Course hoặc chưa enrollment | PUT progress | `400`/`403`/`404`; dữ liệu cũ không đổi. |
| TC-COURSE-F06-001 | F06 AC | Component + integration | Dataset nhỏ, filter xác định | Export | `200 text/csv`; header và record/order đúng filter. |
| TC-COURSE-F06-002 | F06 alternative | Component | Filter hợp lệ không có record | Export | File chỉ có header, không trả JSON envelope. |
| TC-COURSE-F06-003 | F06 boundary/negative | Integration | 100k+ record; cancellation token | Export rồi hủy | Stream/chunk; request hủy dừng xử lý sạch, không materialize toàn CSV hoặc trả lỗi nhạy cảm. |

## Evidence cần thu ở Phase 5

- Unit test cho validation, publish rule, idempotency và progress transition.
- TestServer component test cho route, binding, status, envelope/CSV.
- MySQL Testcontainers integration test cho unique race, transaction và
  persistence.
- NFR CSV/N+1/index/pagination nằm tại
  [non-functional-testcases.md](non-functional-testcases.md).

