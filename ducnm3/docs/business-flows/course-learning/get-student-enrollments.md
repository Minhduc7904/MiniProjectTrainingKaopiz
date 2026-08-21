# `GET /course/api/student/enrollments` — Home Student

API contract: [`get-student-enrollments.md`](../../../api/course-service/endpoints/get-student-enrollments.md)

## Mục tiêu

Cho Student xem trang Home với các Course đã ghi danh, có thumbnail và phân trang ổn định.

## Actor và thành phần

- Student đã login.
- Gateway.
- Course Service và MySQL Course.
- Media Service để lấy thumbnail theo batch.

## Điều kiện trước

- Header actor hợp lệ, type là `STUDENT`.
- `page` và `pageSize` hợp lệ.

## UML luồng chạy

```mermaid
sequenceDiagram
    participant Student
    participant Gateway
    participant Course as Course Service
    participant DB as MySQL Course
    participant Media as Media Service
    Student->>Gateway: GET /course/api/student/enrollments?page=1&pageSize=12
    Gateway->>Course: Forward Student actor
    Course->>DB: Filter enrollments by actor student_id, offset page
    Course->>Media: Batch thumbnail metadata by course IDs
    Media-->>Course: Thumbnails
    Course-->>Gateway: Offset envelope, no-store
    Gateway-->>Student: Enrolled Course cards
```

## Luồng lỗi

- `400 VALIDATION_FAILED`: phân trang sai.
- `403 FORBIDDEN`: actor không phải Student.

## Dữ liệu và side effects

Chỉ đọc `enrollments`, `courses` và metadata Media; không ghi database hay event.

## Test mapping

- Unit: `StudentLearningHandlersTests` kiểm tra pagination và ownership.
- Component: `StudentLearningEndpointsComponentTests` kiểm tra envelope, thumbnail, no-store và actor.
