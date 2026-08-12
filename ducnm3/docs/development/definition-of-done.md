# 53. Definition of Done toàn project

## Infrastructure

- [ ] 4 microservices chạy: Course, Student, Media, Notification.
- [ ] MySQL chạy Docker.
- [ ] MinIO chạy Docker.
- [ ] Docker network đúng.
- [ ] Volume persist data.
- [ ] `.env.example` đầy đủ.

## Course

- [ ] Course CRUD cơ bản.
- [ ] Lesson.
- [ ] Enrollment.
- [ ] Progress.

## Media

- [ ] Chỉ Media Service truy cập MinIO.
- [ ] Upload/download media với metadata và MIME validation.
- [ ] `media_usages` liên kết thumbnail, embed, và attachment.
- [ ] Markdown Course/Lesson/Notification nhúng media qua Media Service URL.

## Batch

- [ ] Background job.
- [ ] Chunking.
- [ ] Retry 1 lần.
- [ ] Failure tracking.
- [ ] Idempotency.
- [ ] Benchmark 3k/10k/100k.

## Performance

- [ ] CSV 100k+.
- [ ] CSV streaming.
- [ ] N+1 demo.
- [ ] N+1 optimized.
- [ ] Index benchmark.
- [ ] `EXPLAIN ANALYZE`.
- [ ] Offset pagination benchmark.
- [ ] Cursor pagination.

## API

- [ ] Validation error.
- [ ] Not Found.
- [ ] Conflict.
- [ ] Internal error.
- [ ] Standard error response.

## Documentation

- [ ] Architecture diagram.
- [ ] Class diagram.
- [ ] Sequence diagram.
- [ ] Activity diagram.
- [ ] Benchmark result.
- [ ] Slide.
- [ ] Demo script.

---
