# 41. UML cần chuẩn bị

Tối thiểu 4 diagram.

## 41.1. Component Diagram

```mermaid
flowchart LR
    Client --> Gateway
    Gateway --> CourseService
    Gateway --> StudentService
    Gateway --> MediaService
    Gateway --> NotificationService
    Gateway --> SchedulerApi

    CourseService --> CourseDB[(Course DB)]
    CourseService --> MediaService

    StudentService --> StudentDB[(Student DB)]

    MediaService --> MediaDB[(Media DB)]
    MediaService --> MinIO[(MinIO)]

    NotificationService --> NotificationDB[(Notification DB)]
    NotificationService --> StudentService
    NotificationService --> MediaService

    SchedulerApi --> SchedulerDB[(Scheduler DB)]
    SchedulerWorker[Scheduler Worker skeleton] -. future .-> SchedulerDB
    SchedulerWorker -. future HTTP .-> NotificationService
    SchedulerWorker -. future HTTP .-> MediaService
```

---

## 41.2. Course Class Diagram

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

## 41.3. Notification Sequence Diagram

```mermaid
sequenceDiagram
    participant Admin
    participant API as Notification API
    participant DB as Notification DB
    participant Worker
    participant Student as Student Service

    Admin->>API: POST /notification-batches
    API->>DB: Create Batch + recipient snapshot
    API-->>Admin: 202 Accepted

    Note over Worker: Future phase; not implemented in foundation
    Worker->>DB: Get pending batch
    Worker->>Student: Get student batch
    Student-->>Worker: 500 students

    loop each batch
        Worker->>Worker: Send notifications
        Worker->>DB: Update item status
    end

    Worker->>DB: Complete batch
```

---

## 41.4. Batch Activity Diagram

```mermaid
flowchart TD
    A[Start Batch] --> B[Load Recipient Chunk]
    B --> C{Has Records?}
    C -- No --> H[Complete Batch]
    C -- Yes --> D[Send Notification]
    D --> E{Success?}
    E -- Yes --> F[Mark SUCCESS]
    E -- No --> G{Retry Count < 1?}
    G -- Yes --> D
    G -- No --> I[Mark FAILED + Save Error]
    F --> B
    I --> B
```

---
