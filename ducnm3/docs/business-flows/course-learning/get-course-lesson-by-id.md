# Lấy chi tiết Lesson

Admin mở right panel chỉnh sửa Lesson. Frontend gửi `GET /course/api/courses/{courseId}/lessons/{lessonId}` để lấy snapshot đầy đủ trước khi render form.

```mermaid
sequenceDiagram
    participant Admin
    participant FE as Frontend
    participant API as Course Service
    participant DB as Course DB
    Admin->>FE: Chọn chỉnh sửa Lesson
    FE->>API: GET Lesson detail
    API->>DB: Read-only projection
    DB-->>API: Lesson hoặc null
    API-->>FE: 200 envelope / 404 error envelope
```

Endpoint không thay đổi database, không publish message và response dùng `no-store`.
