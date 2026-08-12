# 4. Kiến trúc Microservices đề xuất

Sử dụng **4 business services**. Media Service là boundary bắt buộc vì nó sở hữu MinIO, metadata, media usage và quyền truy cập file.

```text
                    ┌───────────────────┐
                    │       Client      │
                    │ React / Swagger   │
                    └─────────┬─────────┘
                              │
                    ┌─────────▼─────────┐
                    │    API Gateway    │
                    │  YARP / Optional  │
                    └──────┬─────┬──────┘
                           │     │
          ┌────────────────┼─────┼────────────────┐
          │                │     │                │
 ┌────────▼────────┐ ┌─────▼──────┐ ┌─────▼──────┐ ┌──────────────▼─────────────┐
 │ Course Service  │ │Student Svc  │ │Media Svc   │ │ Notification Service        │
 │ Course/Lesson   │ │Students     │ │API + MinIO │ │ API + Background Worker     │
 └────────┬────────┘ └─────┬──────┘ └─────┬──────┘ └──────────────┬─────────────┘
          │                │              │                         │
 ┌────────▼────────┐ ┌─────▼──────┐ ┌─────▼──────┐          ┌───────▼────────┐
 │ Course MySQL    │ │Student MySQL│ │Media MySQL │          │Notification DB │
 └─────────────────┘ └────────────┘ └─────┬──────┘          └────────────────┘
                                           │
                                    ┌──────▼──────┐
                                    │    MinIO    │
                                    └─────────────┘
```

---
# 5. Phân chia Service

## 5.1. Course Service

Chịu trách nhiệm:

- Course
- Lesson
- Enrollment
- Lesson Progress
- Markdown source for Course description and Lesson content
- CSV Export
- N+1 demo
- Index demo
- Pagination demo
- Query Plan demo

### Tables

```text
courses
lessons
enrollments
lesson_progresses
```

---

## 5.2. Student Service

Chịu trách nhiệm:

- User/Student
- Student profile
- Student lookup

### Tables

```text
students
```

Có thể seed:

- 3,000 students
- 10,000 students
- 100,000 students

để test batch.

---

## 5.3. Media Service

Chịu trách nhiệm:

- Upload/download object với MinIO.
- Metadata cho image, video, document, audio và các loại media khác.
- Presigned/proxy URL, content type validation, size limit, và soft delete.
- Liên kết media với Course description, Lesson content, Notification body, thumbnail hoặc attachment.
- Quản lý `media_usages`; Course và Notification Service không truy cập MinIO hoặc Media database trực tiếp.

### Tables

```text
media_objects
media_usages
```

---

## 5.4. Notification Service

Chịu trách nhiệm:

- Broadcast notification
- Gửi notification đơn lẻ
- In-app inbox và read status của student
- Background batch processing
- Retry
- Idempotency
- Failure tracking
- Batch benchmark

### Tables

```text
notification_jobs
notification_job_items
notifications
```

---
# 6. Vì sao chỉ tách 4 Service?

Trong 5 ngày:

```text
User Service
Course Service
Lesson Service
Enrollment Service
Progress Service
Notification Service
Export Service
```

là **over-engineering**.

Microservice không có nghĩa là mỗi entity thành một service.

Boundary nên theo **business capability**:

```text
Course Service
Student Service
Media Service
Notification Service
```

Đủ để:

- Có service boundary.
- Có database ownership.
- Có network communication.
- Có Docker network.
- Có Clean Architecture bên trong từng service.
- Không làm mất 3 ngày chỉ để cấu hình infrastructure.

---
# 7. Database ownership

Nguyên tắc:

> Mỗi service sở hữu database/schema của riêng nó.

Trong demo có thể dùng **một MySQL container**, nhưng tạo database riêng:

```text
lms_course_db
lms_student_db
lms_media_db
lms_notification_db
```

Không nên:

```text
Course Service
    │
    └── query trực tiếp students table
```

Nên:

```text
Course Service
    │
    └── HTTP → Student Service
```

Tương tự, Course Service và Notification Service chỉ gọi HTTP tới Media Service để upload, lấy URL, hoặc đăng ký `media_usages`; tuyệt đối không gọi MinIO hay query `lms_media_db` trực tiếp.

Để vẫn hoàn thành trong 5 ngày, chỉ implement các HTTP contract cần thiết cho media usage và batch recipient lookup; không mở rộng sang event bus hoặc đồng bộ dữ liệu phức tạp.

---
