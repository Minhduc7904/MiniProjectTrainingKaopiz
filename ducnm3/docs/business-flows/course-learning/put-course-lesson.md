# Cập nhật Lesson

`PUT /course/api/courses/{courseId}/lessons/{lessonId}` chỉ đổi các field có trong request. Khi `contentMarkdown` là `null` hoặc rỗng, nội dung được xóa và toàn bộ media usage cũ được gửi cho worker để gỡ. Field không xuất hiện không tạo media command.

```mermaid
sequenceDiagram
    participant Admin
    participant Course as Course Service
    participant DB as Course DB
    participant Worker as Media Worker
    Admin->>Course: PUT Lesson partial payload
    Course->>DB: Read and update Lesson
    Course->>Worker: synchronize LESSON_CONTENT diff
    Course-->>Admin: 200 response envelope
```
