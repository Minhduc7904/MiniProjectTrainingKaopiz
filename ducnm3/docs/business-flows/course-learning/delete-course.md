# Xóa Course

API contract: [`DELETE Course`](../../api/course-service/endpoints/delete-course.md).

## Mục tiêu

Xóa hẳn Course cùng toàn bộ Lesson phụ thuộc, đồng thời yêu cầu Media Service dọn mọi media usage của Course và Lesson.

## Luồng

```mermaid
sequenceDiagram
    participant Admin
    participant Course as Course Service
    participant Media as Media Service
    participant DB as Course DB
    participant Worker as Media Worker
    Admin->>Course: DELETE /api/courses/{courseId}
    Course->>DB: đọc Course và Lesson IDs
    Course->>Media: query usage IDs theo scope Course/Lesson
    Course->>DB: hard-delete Course (cascade Lesson/progress/enrollment)
    Course->>Worker: DeleteMediaUsagesByIdsV1
    Course-->>Admin: 202 Accepted
    Worker->>Media: soft-delete media usages theo batch
```

## Quy tắc

- Scope Course: `COURSE_DESCRIPTION`, `COURSE_THUMBNAIL`, `COURSE_GALLERY`.
- Scope từng Lesson: `LESSON_CONTENT`, `LESSON_ATTACHMENT`.
- Media usage không tồn tại/đã xóa khi worker xử lý được bỏ qua để command idempotent.
- Không có distributed transaction; nếu command lỗi sau hard-delete, broker retry theo policy Media Worker.
