# Thiết kế Student demo auth và learning flow

## Mục tiêu

Thêm luồng Student tối giản cho môi trường demo: đăng ký, đăng nhập bằng UUID,
kiểm tra phiên qua `me`, ghi danh Course và hoàn thành Lesson. Không dùng JWT,
cookie, password hoặc session server-side.

## Phạm vi

- Student Service sở hữu register, login và `me`.
- Course Service sở hữu Enrollment và LessonProgress.
- Frontend tách hoàn toàn actor Admin và Student trong localStorage.
- Toàn bộ SPA quản trị hiện có chuyển dưới `/admin`; khu vực Student nằm dưới
  `/student` và chỉ render sau khi `me` xác nhận actor.

Không thay Gateway API prefix: public API vẫn là `/student/api/...` và
`/course/api/...`.

## Identity demo

Frontend lưu Student dưới `lms.student.actor` với shape:

```json
{ "type": "STUDENT", "id": "11111111-1111-1111-1111-111111111111" }
```

Admin dùng key riêng `lms.admin.actor`; không còn dùng một `lms.actor` chung.
HTTP client Student gửi `X-Actor-Type: STUDENT` và `X-Actor-Id`; HTTP client
Admin chỉ gửi actor Admin. Header này chỉ là identity giả lập Development: client
có thể giả mạo nên không được dùng làm cơ chế bảo mật Production.

## API contract

| Endpoint | Thành công | Lỗi nghiệp vụ |
| --- | --- | --- |
| `POST /student/api/auth/register` body `{ email, displayName }` | `201` envelope `{ actor: "STUDENT", id }` | `400`, `409 STUDENT_EMAIL_EXISTS` |
| `POST /student/api/auth/login` body `{ id }` | `200` envelope `{ actor: "STUDENT", id }` | `400`, `403 STUDENT_NOT_ACTIVE`, `404 STUDENT_NOT_FOUND` |
| `GET /student/api/auth/me` headers Student actor | `200` envelope profile Student | `400` header sai, `403` actor khác hoặc Student không ACTIVE, `404` Student không có |
| `POST /course/api/courses/{courseId}/enrollments` | `201` envelope Enrollment | `400`, `403` actor không phải Student hoặc Course chưa `PUBLISHED`, `404` Course không có, `409 ENROLLMENT_ALREADY_EXISTS` |
| `POST /course/api/courses/{courseId}/lessons/{lessonId}/progress/complete` | `200` envelope progress `100` | `400`, `403` actor/enrollment sai, `404` Course/Lesson sai ownership, `409` khi state dữ liệu mâu thuẫn |

`register` chỉ nhận `email` và `displayName`; service sinh UUID, tạo Student
`ACTIVE`. `login` nhận UUID, không có password, và chỉ chấp nhận Student
`ACTIVE`. `me` kiểm tra header chuẩn hóa và đọc Student DB.

Enrollment không có request body, lấy Student ID từ actor header. Course phải
`PUBLISHED`; unique constraint `(course_id, student_id)` bảo vệ duplicate.
Complete progress không có request body, yêu cầu active enrollment và Lesson
thuộc Course; upsert theo unique `(lesson_id, student_id)`, luôn đặt
`progress_percent = 100` và chỉ tạo `completed_at` ở lần hoàn thành đầu. Gọi
lại endpoint là idempotent ở mức business và trả representation hiện tại.

## Frontend routing

```text
/admin/*                 Khu vực quản trị hiện tại; actor Admin riêng
/student/register        Public: tạo Student
/student/login           Public: login bằng UUID
/student/loading         Guarded bootstrap: gọi /auth/me
/student/logout          Xóa student storage, replace sang /student/login
/student/*               Guarded Student pages
```

`StudentRouteGuard` chuyển client không có `lms.student.actor` sang login. Nếu
có actor nhưng profile chưa được xác nhận trong phiên, guard chuyển tới loading
và giữ `returnTo`. Loading gọi `me`; `200` cache profile trong Redux/context rồi
đi tới `returnTo`; `400`/`403`/`404` xóa Student storage và replace sang login.
Loading truy cập trực tiếp không có actor cũng redirect login.

Register/login ghi actor Student rồi navigate bằng `replace` tới loading.
Logout chỉ xóa actor Student, không xóa Admin actor, rồi replace tới login.

## Kiến trúc và dữ liệu

Student Application có use case `Register`, `Login`, `GetMe`; Infrastructure
mở rộng repository EF hiện có để tạo Student, tra cứu email và tra cứu profile.
Course Application có `Enroll` và `CompleteLessonProgress`, abstractions riêng
và EF adapter dùng transaction/unique constraint hiện có. Không cần migration:
`students`, `enrollments` và `lesson_progresses` đã chứa toàn bộ field/index
cần thiết.

Endpoint chỉ bind HTTP/map response; Application sở hữu validation, state rule
và error code; Infrastructure sở hữu EF Core. Actor policy chung tiếp tục parse
header, Course endpoint dùng `RequireActor(ActorAccess.Student)`.

## UI direction

Giữ design system "Quiet admin" hiện có: Source Sans 3, canvas lạnh, accent
teal, borders-only và spacing 4px. Auth screen là form tập trung, một action
chính rõ ràng, các trạng thái loading/error dùng component/tokens hiện có; không
thêm palette hay dependency UI mới.

## Kiểm thử và verification

- Unit: validation, Student status, duplicate email, Enrollment/Progress state
  rule và idempotent completion với fake repository.
- Component: TestServer cover payload/header binding, success, error envelope,
  actor policy và exact route/status.
- Integration: MySQL Testcontainer áp dụng migration production, xác minh
  unique email/enrollment/progress và persistence của complete progress.
- Frontend Vitest: storage tách Admin/Student, auth redirect state machine,
  loading `me` success/failure và logout.
- Cập nhật API docs, business flows, test catalog và Postman; chạy focused test,
  backend build/test/format, frontend test/lint/build và JSON validation.

## Ngoài phạm vi

- JWT, password hash, refresh token, cookie/session server-side và OAuth.
- Production-grade authorization hoặc chống giả mạo actor header.
- Migration schema mới, notification, payment, lesson content authoring hay UI
  học tập ngoài auth/guard cần thiết cho luồng này.
