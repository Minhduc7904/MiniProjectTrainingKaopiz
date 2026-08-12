# DAY 2 — LMS Core + MinIO + N+1

## Goal

Có LMS core đủ để demo query optimization.

### Task

#### Course

- Create Course.
- List Course.
- Create Lesson.
- Enrollment.
- Progress.

#### MinIO

- Media Service tạo `media_objects` và `media_usages` để lưu metadata, loại media, và vị trí sử dụng.
- Upload thumbnail/tài liệu qua Media Service; Course Service không gọi MinIO.
- Course Service lưu Markdown và gọi Media Service để liên kết thumbnail, embed, hoặc attachment.
- Download/presigned URL qua Media Service.

#### N+1

Tạo:

```text
GET /courses/details-naive
GET /courses/details-optimized
```

Naive:

```text
N+1
```

Optimized:

```text
Projection / Join
```

Bật SQL logging.

### Seed

```text
100 courses
20 lesson/course
progress records
```

### Definition of Done Day 2

- [ ] CRUD Course chạy.
- [ ] Lesson chạy.
- [ ] Progress chạy.
- [ ] Upload/download MinIO chạy.
- [ ] Course hoặc Lesson dùng được nhiều loại media qua Media Service.
- [ ] Có endpoint N+1.
- [ ] Có endpoint optimized.
- [ ] Có log SQL để so query count.

---
