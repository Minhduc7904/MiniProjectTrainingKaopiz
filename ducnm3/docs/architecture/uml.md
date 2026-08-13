# 41. Các sơ đồ UML cần chuẩn bị

Tối thiểu 4 sơ đồ.

## 41.1. Sơ đồ thành phần

```mermaid
flowchart LR
    Client --> Gateway
    Gateway --> CourseService
    Gateway --> StudentService
    Gateway --> MediaService
    Gateway --> NotificationService
    Gateway --> SchedulerApi

    CourseService --> CourseDB[(CSDL Khóa học)]
    CourseService --> MediaService

    StudentService --> StudentDB[(CSDL Học viên)]

    MediaService --> MediaDB[(CSDL Media)]
    MediaService --> MinIO[(MinIO)]

    NotificationService --> NotificationDB[(CSDL Thông báo)]
    NotificationService --> StudentService
    NotificationService --> MediaService

    SchedulerApi --> SchedulerDB[(CSDL Scheduler)]
    SchedulerWorker[Khung Scheduler Worker] -. tương lai .-> SchedulerDB
    SchedulerWorker -. HTTP trong tương lai .-> NotificationService
    SchedulerWorker -. HTTP trong tương lai .-> MediaService
```

---

## 41.2. Sơ đồ lớp Course

```mermaid
classDiagram
    class Course {
        +Guid Id
        +string Name
        +CourseStatus Status
        +DateTime CreatedAt
    }

    class Lesson {
        +Guid Id
        +Guid CourseId
        +string Title
        +int Order
    }

    class Enrollment {
        +Guid Id
        +Guid CourseId
        +Guid StudentId
    }

    class LessonProgress {
        +Guid Id
        +Guid LessonId
        +Guid StudentId
        +int ProgressPercent
    }

    Course "1" --> "*" Lesson
    Course "1" --> "*" Enrollment
    Lesson "1" --> "*" LessonProgress
```

---

## 41.3. Sơ đồ tuần tự Thông báo

```mermaid
sequenceDiagram
    participant Admin as Quản trị viên
    participant API as API Thông báo
    participant DB as CSDL Thông báo
    participant Worker as Tiến trình nền
    participant Student as Student Service

    Admin->>API: POST /notification-batches
    API->>DB: Tạo lô + chụp danh sách người nhận
    API-->>Admin: 202 Đã chấp nhận

    Note over Worker: Giai đoạn tương lai; phần nền tảng chưa triển khai
    Worker->>DB: Lấy lô đang chờ
    Worker->>Student: Lấy lô Học viên
    Student-->>Worker: 500 Học viên

    loop từng lô
        Worker->>Worker: Gửi thông báo
        Worker->>DB: Cập nhật trạng thái mục
    end

    Worker->>DB: Hoàn tất lô
```

---

## 41.4. Sơ đồ hoạt động xử lý hàng loạt

```mermaid
flowchart TD
    A[Bắt đầu lô] --> B[Tải nhóm người nhận]
    B --> C{Có bản ghi?}
    C -- Không --> H[Hoàn tất lô]
    C -- Có --> D[Gửi thông báo]
    D --> E{Thành công?}
    E -- Có --> F[Đánh dấu SUCCESS]
    E -- Không --> G{Số lần thử lại < 1?}
    G -- Có --> D
    G -- Không --> I[Đánh dấu FAILED + Lưu lỗi]
    F --> B
    I --> B
```

---
