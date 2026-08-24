# `GET /course/api/student/courses` — Danh mục Course Student

API contract: [`get-student-course-catalog.md`](../../../api/course-service/endpoints/get-student-course-catalog.md)

## Mục tiêu

Cho Student khám phá và ghi danh các Course mới; Course đã ghi danh không lặp lại trong catalog.

## Actor và thành phần

- Student đã login.
- Gateway.
- Course Service, MySQL Course và Media Service.

## Điều kiện trước

- Actor header hợp lệ, type là `STUDENT`.
- Search (nếu có) được trim và pagination hợp lệ.

## UML luồng chạy

```mermaid
sequenceDiagram
    participant Student
    participant Gateway
    participant Course as Course Service
    participant DB as MySQL Course
    participant Media as Media Service
    Student->>Gateway: GET /course/api/student/courses?search=Backend&page=1&pageSize=12
    Gateway->>Course: Forward Student actor
    Course->>DB: Read PUBLISHED courses matching name without actor enrollment
    Course->>Media: Read thumbnails by course IDs
    Media-->>Course: Thumbnail metadata
    Course-->>Student: Offset catalog envelope, no-store
```

## Luồng lỗi

- `400 VALIDATION_FAILED`: phân trang sai.
- `403 FORBIDDEN`: actor không phải Student.

## Dữ liệu và side effects

Chỉ đọc `courses`, `enrollments` và metadata Media. Search tên Course kết hợp `AND` với điều kiện PUBLISHED/chưa ghi danh; chuỗi rỗng không lọc. Ghi danh là thao tác riêng qua `POST /course/api/courses/{courseId}/enrollments`; frontend thay route sang detail khi POST thành công.

## Test mapping

- Unit: `StudentLearningHandlersTests`.
- Component: `StudentLearningEndpointsComponentTests`.
