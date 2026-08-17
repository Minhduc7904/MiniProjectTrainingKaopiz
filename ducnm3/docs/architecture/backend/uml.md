# UML Backend

~~~mermaid
flowchart LR
  Client --> Gateway
  Gateway --> Course
  Gateway --> Student
  Gateway --> Media
  Gateway --> Notification
  Gateway --> Scheduler
  Media --> MinIO
  Course --- RabbitMQ
  Student --- RabbitMQ
  Media --- RabbitMQ
  Notification --- RabbitMQ
  Scheduler --- RabbitMQ
~~~

~~~mermaid
classDiagram
  Course "1" --> "*" Lesson
  Course "1" --> "*" Enrollment
  Lesson "1" --> "*" LessonProgress
~~~

Scheduler execution flow không có sơ đồ vì Cron/polling/handler chưa được triển khai.

