# API Design Matrix

## Quy ước

- `Existing`: có runtime evidence theo Function List Phase 3.
- `Planned`: contract/design phục vụ Phase 5, chưa claim endpoint chạy.
- Direct path bắt đầu bằng `/api`; public path thêm Gateway prefix của service.
- Mỗi endpoint khi implement phải có đúng một API doc và một business-flow file
  1:1 theo `rules/api-documentation.md`.

## Course

| Function | Operation/direct path | Success | Trạng thái | Contract/design |
| --- | --- | --- | --- | --- |
| F01 | `POST /api/courses` | `201` + Course | Planned | [Create Course](../../api/course-service/endpoints/post-courses.md) |
| F01 | `PATCH /api/courses/{courseId}` | `200` + Course | Planned design gap | Typed partial update: allow `name`, `descriptionMarkdown`, `status`; absent giữ nguyên; null chỉ clear description; `{}` là no-op; transition publish phải kiểm tra điều kiện. |
| F02 | `POST /api/courses/{courseId}/lessons` | `201` + Lesson | Planned | [Create Lesson](../../api/course-service/endpoints/post-course-lessons.md) |
| F02 | `PATCH /api/lessons/{lessonId}` | `200` + Lesson | Planned design gap | Allow `title`, `displayOrder`, `contentMarkdown`; unique order trong Course; field absent giữ nguyên. |
| F03 | `GET /api/courses`, `/cursor` | `200` paged list | Planned | [Offset](../../api/course-service/endpoints/get-courses.md), [cursor](../../api/course-service/endpoints/get-courses-cursor.md) |
| F03 | `GET /api/courses/{courseId}`, `/details` | `200` detail | Planned | [Course](../../api/course-service/endpoints/get-course-by-id.md), [details](../../api/course-service/endpoints/get-course-details.md) |
| F04 | `POST /api/courses/{courseId}/enrollments` | `201` + Enrollment | Planned design gap | Identity lấy từ authentication boundary; business key `(courseId, studentId)`; retry cùng actor/course không tạo row thứ hai; `Location` và replay status cần chốt ở Q4-02. |
| F05 | `PUT /api/courses/{courseId}/lessons/{lessonId}/progress` | `200` + Progress | Planned design gap | Full representation gồm `progressPercent`; URI xác định Course/Lesson/current Student; cùng value là idempotent; `100` đặt `completedAt`, replay không đổi timestamp. |
| F06 | `GET /api/courses/export` | `200 text/csv` | Planned | [CSV export](../../api/course-service/endpoints/get-courses-export.md) |

PATCH Course/Lesson hiện chọn last accepted write trong phạm vi MiniProject vì
schema chưa có version token. Unique constraint vẫn bảo vệ invariant. Nếu Lead
yêu cầu optimistic concurrency, phải cập nhật database design/migration trước
G3 thay vì tự thêm `If-Match` chỉ trong tài liệu API.

## Student

| Function | Operation/direct path | Success | Trạng thái | Contract |
| --- | --- | --- | --- | --- |
| F07 | `GET /api/students` | `200` offset page | Existing | [Student list](../../api/student-service/endpoints/get-students.md) |
| F07 | `GET /api/students/course/{courseId}` | `200` cursor page | Existing/internal | [Course students](../../api/student-service/endpoints/get-course-students.md) |
| F08 | `GET /api/students/{studentId}` | `200` Student | Existing | [Student detail](../../api/student-service/endpoints/get-student-by-id.md) |

## Media

| Function | Operations | Trạng thái | Contract index |
| --- | --- | --- | --- |
| F09 | `POST /api/media`, thumbnail retry | Existing | [Media API](../../api/media-service/README.md) |
| F10 | GET metadata/content/thumbnail/usage URL | Existing | [Media API](../../api/media-service/README.md) |
| F11 | POST/GET/DELETE usage | Existing | [Media API](../../api/media-service/README.md) |

Binary response không dùng JSON envelope. Metadata/URL response không được trả
bucket hoặc object key. Thumbnail `FAILED` không biến media gốc `READY` thành
failure.

## Notification

| Function | Operations | Trạng thái | Contract index |
| --- | --- | --- | --- |
| F12 | `POST /api/notifications` | Existing | [Notification API](../../api/notification-service/README.md) |
| F13 | GET inbox/detail, PATCH read, POST read-all | Planned một phần | [Notification API](../../api/notification-service/README.md) |
| F14 | `POST /api/notification-batches` | Existing | [Create batch](../../api/notification-service/endpoints/post-notification-batches.md) |
| F17 | GET batch và failed-items | Existing | [Notification API](../../api/notification-service/README.md) |

F15–F16 là worker function, không tạo public HTTP endpoint. POST batch trả
`202 Accepted` và `Location` tới GET batch; accepted không có nghĩa dispatch đã
hoàn thành.

## Scheduler

F18 không mở endpoint cleanup công khai. Scheduler tạo job run và gửi command
nội bộ cho Media owner; client chỉ dùng health/operational surface đã được duyệt.
Contract command nằm tại [Messaging và worker design](messaging-worker-design.md).

## Error baseline

| Nhóm | Hành vi |
| --- | --- |
| Validation | `400`, reject trước side effect, details theo field khi an toàn. |
| Authentication/authorization | `401`/`403`; không dùng ID trong payload thay cho identity đáng tin cậy. |
| Not found | `404`; không trả resource khác hoặc lộ ownership. |
| Business/concurrency conflict | `409` với business error code ổn định. |
| Dependency | `5xx/503` theo shared error contract; không lộ SQL, host, credential hoặc storage key. |

