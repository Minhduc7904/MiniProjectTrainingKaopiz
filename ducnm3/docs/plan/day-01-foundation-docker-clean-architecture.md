# DAY 1 — Foundation + Docker + Clean Architecture

## Goal

Cuối ngày:

```text
docker compose up
```

phải chạy được:

- MySQL
- MinIO
- Course Service
- Student Service
- Media Service
- Notification Service

### Task

#### 1. Khởi tạo solution

```text
LmsMini.sln
```

#### 2. Tạo 4 service

```text
Course
Student
Media
Notification
```

#### 3. Setup Clean Architecture

Mỗi service:

```text
Api
Application
Domain
Infrastructure
```

#### 4. Setup EF Core + MySQL

Database:

```text
lms_course_db
lms_student_db
lms_media_db
lms_notification_db
```

#### 5. Dockerize

- Dockerfile từng service.
- `docker-compose.yml`.
- Network.
- Volumes.
- Environment.

#### 6. MinIO

- Add MinIO container.
- Create bucket.
- Media Service kết nối MinIO và test upload 1 file.

### Definition of Done Day 1

- [ ] `docker compose up` chạy toàn stack.
- [ ] Course API truy cập được Swagger.
- [ ] Media API truy cập được Swagger.
- [ ] MySQL connect thành công.
- [ ] MinIO Console truy cập được.
- [ ] Migration chạy thành công.
- [ ] Upload test file vào MinIO thành công.

---
