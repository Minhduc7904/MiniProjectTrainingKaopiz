# DAY 3 — Batch + Retry + Idempotency

## Goal

Demo background processing rõ ràng.

### Task

#### Student Seed

Generate:

```text
3k
10k
100k
```

students.

#### Notification Job

```http
POST /notification-jobs
```

Flow:

```text
Create Job
→ Return 202
→ Worker Process
```

#### Single notification and inbox

```http
POST  /notifications
GET   /notifications/me
PATCH /notifications/{id}/read
```

- Gửi đơn lẻ tạo một inbox item cho đúng một student.
- Student chỉ xem và đánh dấu đã đọc notification của chính mình.
- Batch worker cũng tạo inbox item cho từng recipient.
- Notification body lưu Markdown và đăng ký media embed/attachment qua Media Service.

#### Chunk

```text
batchSize = 500
```

#### Retry

```text
retry 1 lần
```

#### Failure

Save:

```text
status
retry_count
error_message
```

#### Idempotency

```text
UNIQUE(job_id, student_id)
```

### Definition of Done Day 3

- [ ] API return 202.
- [ ] Job chạy background.
- [ ] Batch 500.
- [ ] Retry chạy.
- [ ] Failed item được lưu.
- [ ] Job progress xem được.
- [ ] Restart không tạo duplicate successful item.
- [ ] Gửi đơn lẻ chạy và student đọc/đánh dấu đã đọc notification của mình.
- [ ] Batch không tạo notification trùng khi worker retry hoặc restart.
- [ ] Notification Markdown render media thông qua Media Service URL.

---
