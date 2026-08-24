# Checklist chuẩn bị và chạy demo

## 1. Nguyên tắc demo

- Demo luồng nhỏ, dùng evidence có sẵn cho dataset lớn.
- Không seed/reset hoặc chạy batch 100k trực tiếp trong buổi nói.
- Mỗi request demo dùng Correlation ID dễ nhớ.
- Mỗi bước phải có một phương án fallback: raw JSON, Postman response đã lưu
  hoặc ảnh chụp màn hình không chứa secret.
- Không mở `.env`, connection string, credential MinIO/RabbitMQ hoặc raw message
  body nhạy cảm trên máy chiếu.

## 2. Một ngày trước buổi trình bày

### Hệ thống

- [ ] Checkout đúng commit/branch sẽ demo; ghi commit SHA vào note.
- [ ] `docker compose config` pass.
- [ ] Build backend pass.
- [ ] Frontend build/test cần thiết pass.
- [ ] Các container chính healthy.
- [ ] Console log, RabbitMQ Management và MinIO Console mở được.
- [ ] `mysql-init` và `minio-init` đã completed thành công.
- [ ] MinIO có đủ năm bucket và Media health trả healthy.
- [ ] Database có dataset demo ổn định.
- [ ] Có tài khoản Admin/Student demo, không dùng credential production.

```powershell
docker compose config
dotnet build backend/Lms.sln -m:1
npm --prefix frontend/lms-web run build
docker compose up -d --build
docker compose ps
```

### Performance

- [ ] Không sử dụng CSV 10k/100k hiện tại làm kết luận vì row count mismatch.
- [ ] Nếu đã rerun CSV, kiểm tra row/bytes/hash trước khi thay bảng slide.
- [ ] Nếu chưa làm N+1 harness, để response time là `Chưa đo`.
- [ ] Index giữ placeholder.
- [ ] Copy raw evidence cần trình bày sang thư mục backup read-only hoặc USB.

### Postman và FE

- [ ] Import `postman/MiniProjectKaopiz.postman_collection.json`.
- [ ] Chọn đúng local environment/base URL.
- [ ] Xóa token/secret cũ khỏi console/history hiển thị.
- [ ] Chuẩn bị sẵn Course ID, Lesson ID, Media ID, Student ID hợp lệ.
- [ ] Mở sẵn các tab FE; kiểm tra không có route placeholder nằm trong demo.

## 3. Cửa sổ cần mở theo thứ tự

| Số | Cửa sổ | Nội dung |
| ---: | --- | --- |
| 1 | Kịch bản Markdown | Mở `01-kich-ban-thuyet-trinh.md` ở phần hiện tại. |
| 2 | Frontend Admin | Dashboard, Course, Media Library, Notification Batch. |
| 3 | Frontend Student | Login/Home/Catalog/Course/Lesson. |
| 4 | Postman | Collection local, request batch/course details. |
| 5 | Terminal log | `docker compose logs -f` cho Gateway, API và Worker. |
| 6 | RabbitMQ Management | Queues, consumers, `_error`. |
| 7 | MinIO Console | Năm bucket và một object key an toàn để minh họa. |
| 8 | DB client | Notification/Media tables và MassTransit persistence. |
| 9 | Terminal | `docker compose ps/logs/stats`, PerformanceRunner output. |
| 10 | IDE | Các file code/skill đã bookmark. |

## 4. Bookmark code và tài liệu

### Quy trình/skill

- `.agents/skills/api-post-endpoint/SKILL.md`
- `docs/guide/DEVKIT_GUIDE.md`
- `docs/guide/DEV_TASK_GUIDE.md`
- `postman/MiniProjectKaopiz.postman_collection.json`

### Kiến trúc

- `docs/architecture/backend/overview.md`
- `docs/architecture/frontend/architecture.md`
- `docs/architecture/backend/shared/clean-architecture.md`
- `docs/architecture/backend/shared/service-communication.md`
- `docker-compose.yml`
- `backend/Dockerfile`
- `scripts/storage/minio/init-media-storage.sh`
- `docs/presentation/07-phao-docker-minio.md`

### Bài toán chính

- `docs/business-flows/notifications/bulk-notification.md`
- `docs/business-flows/courses/get-courses-export.md`
- `docs/development/minio.md`
- `docs/database/notification-service/data-model.md`
- `docs/database/media-service/data-model.md`
- `docs/database/course-service/data-model.md`

### Code

- Batch create/dispatch handler và repository.
- `EfCourseDetailsRepository.cs` cho N+1.
- `ExportCoursesHandler.cs` cho CSV.
- `LmsLoggingExtensions.cs` và `HttpLoggingExtensions.cs`.
- `backend/Tools/Lms.PerformanceRunner/`.

## 5. Kiểm tra ngay trước giờ nói

```powershell
docker compose ps
curl.exe -fsS http://localhost:5100/course/health
curl.exe -fsS http://localhost:5100/student/health
curl.exe -fsS http://localhost:5100/media/health
curl.exe -fsS http://localhost:5100/notification/health
```

- [ ] FE ở đúng base URL Gateway.
- [ ] Browser DevTools Network không giữ request nhạy cảm từ trước.
- [ ] Console của Gateway, API và Worker có log mới.
- [ ] RabbitMQ queues có consumer cho Notification/Media Worker.
- [ ] Không có queue `_error` cũ làm khán giả hiểu nhầm; nếu có, biết rõ nguyên
  nhân và không tự purge khi chưa xác nhận.
- [ ] Batch demo cũ không đang `PROCESSING`.
- [ ] Media demo đã `READY` và thumbnail/job có trạng thái dự kiến.
- [ ] Font/zoom terminal và browser đủ lớn.
- [ ] Tắt notification cá nhân và ứng dụng chat.

## 6. Run sheet demo

### Bước A — Tổng quan và FE

1. Admin Dashboard.
2. Course list/detail.
3. Media Library preview.
4. Student login → Home → Catalog → Course/Lesson.

**Fallback:** ảnh chụp từng màn hình + mô tả route/response; không dừng để sửa
CSS hoặc seed data trên sân khấu.

### Bước B — Quy trình và skill

1. Mở DevKit guide ở command `validate/doctor/start/status/verify`.
2. Mở API POST skill.
3. Cuộn qua checklist `API doc → business flow → tests → Postman`.
4. Mở một API doc và business flow tương ứng để chứng minh 1:1.
5. Mở request tương ứng trong Postman.

### Bước C — Kiến trúc

1. Hiện sơ đồ overview.
2. Giải thích ownership table.
3. Mở `docker compose ps` và chỉ ba nhóm: application, worker, infrastructure.
4. Giải thích một physical MySQL local nhưng nhiều logical database theo owner.
5. Chỉ startup chain:

   ```text
   healthcheck → init job → API/Worker
   ```

6. Mở `docker-compose.yml` tại `minio`/`minio-init`, sau đó mở MinIO Console:
   - `images`;
   - `videos`;
   - `documents`;
   - `audios`;
   - `other`.
7. Chỉ một object key an toàn và giải thích:

   ```text
   yyyy/MM/dd/{uuid:N}.{extension}
   ```

8. Đối chiếu `media_objects.bucket/object_key/status/checksum_sha256`; không mở
   credential, signed policy hoặc raw object nhạy cảm.
9. Mở một vertical slice Notification:

   ```text
   Endpoint/Consumer → Handler → Repository port → EF repository → DB
   ```

10. Chỉ typed HTTP cho Student query và command cho Worker/Media.

### Bước D — Batch Notification live

1. Đặt Correlation ID `demo-final-batch-001`.
2. Tạo batch ở mức nhỏ phù hợp dataset/thời gian.
3. Chỉ `202`, `Location`, `batchId`.
4. Mở progress page và quan sát:

   ```text
   PENDING → SNAPSHOTTING → SNAPSHOT_READY → PROCESSING → terminal
   ```

5. Mở console log và lọc theo Correlation ID hoặc `batchId`.
6. Mở RabbitMQ queue/consumer nếu đủ thời gian.
7. Query DB bằng `batchId`.

SQL tham khảo trong DB client:

```sql
SELECT id, status, requested_count, total_count,
       processed_count, success_count, failed_count,
       batch_size, started_at, completed_at
FROM notification_batches
WHERE id = '<batch-id>';

SELECT status, COUNT(*) AS item_count
FROM notification_batch_items
WHERE batch_id = '<batch-id>'
GROUP BY status;
```

**Không** poll thủ công nhiều tab cùng lúc; FE đã có thứ tự Snapshot → Delivery
→ Media Usage.

### Bước E — Outbox/Inbox

Tên đúng là `InboxState`, không phải `InputState`. Xem note chuyên sâu tại
[Giải thích Outbox và Inbox](05-outbox-inbox-notes.md).

Chỉ structure/data an toàn, không mở raw Body nếu notification có dữ liệu nhạy
cảm:

```sql
SELECT SequenceNumber, MessageId, CorrelationId,
       DestinationAddress, EnqueueTime, SentTime
FROM OutboxMessage
ORDER BY SequenceNumber DESC
LIMIT 10;

SELECT OutboxId, Created, Delivered, LastSequenceNumber
FROM OutboxState
ORDER BY Created DESC
LIMIT 10;

SELECT MessageId, ConsumerId, Received, ReceiveCount,
       Consumed, Delivered
FROM InboxState
ORDER BY Received DESC
LIMIT 10;
```

Nói rõ bảng có thể được cleanup theo retention nên không phải lúc nào row cũ
cũng còn.

### Bước F — Debug

Console command:

```powershell
docker compose logs --since 10m api-gateway notification-service notification-worker |
  Select-String -Pattern 'demo-final-batch-001|MessageId|Warning|Error|Exception'
```

Nếu cần mô phỏng lỗi, dùng một validation error an toàn thay vì làm hỏng
RabbitMQ/DB:

1. Gửi request invalid có Correlation ID `demo-final-error-001`.
2. Chỉ HTTP `400`, `meta.traceId` và Warning log.
3. Giải thích với lỗi Worker sẽ tiếp tục từ `MessageId → queue → Inbox/Outbox →
   business state`.

Không chủ động tắt broker hoặc kill Worker trong demo chính trừ khi đã rehearsal
và có thời gian recovery.

### Bước G — CSV

1. Demo download dataset nhỏ bằng FE.
2. Mở code streaming: projection, chunk 500, keyset, writer, cancellation.
3. Mở PerformanceRunner live table hoặc raw JSON đã kiểm chứng.
4. Trình bày TTFB/memory/correctness metric.
5. Nếu CSV chưa rerun hợp lệ, nói rõ evidence mismatch; không mở bảng trend giả.

### Bước H — Media

1. Upload file demo nhỏ.
2. Chỉ `mediaId`, status và content URL; không mở object key/credential.
3. Với ảnh/video/PDF, theo dõi thumbnail job.
4. Mở Media Library/Job list để chỉ state.
5. Giải thích `media_objects`, `media_usages`, `media_background_jobs`.

**Fallback:** dùng media `READY` có sẵn và raw job record; không upload file lớn
trên Wi-Fi không ổn định.

### Bước I — Performance/N+1/Index

1. Mở Batch evidence table.
2. Giải thích metric, không chỉ đọc số.
3. Chỉ audit issue CSV.
4. Mở hai method N+1/optimized và nói query count `2 + L` so với tối đa 3.
5. Index: để placeholder và nói chưa đủ evidence.

## 7. Phương án fallback theo sự cố

| Sự cố | Quyết định trong 15 giây | Fallback |
| --- | --- | --- |
| FE không tải | Kiểm tra Gateway health một lần | Dùng Postman + ảnh FE. |
| Gateway/service down | Không rebuild giữa demo | Dùng architecture/raw response/evidence. |
| Batch không terminal | Giữ batch ID | Trình bày state hiện tại + raw batch result đã lưu. |
| Không thấy console log | Kiểm tra đúng service/container | `docker compose ps`; sau đó `docker compose logs --since 10m <service>`. |
| RabbitMQ UI lỗi | Không sửa credential trên máy chiếu | Dùng console MessageId, DB state và sơ đồ Outbox/Inbox. |
| Upload lỗi mạng | Không retry file lớn | Dùng media READY có sẵn. |
| CSV download lâu | Hủy request | Dùng raw result đã kiểm chứng. |
| ID demo sai | Không sửa DB trực tiếp | Dùng ID backup đã ghi trong note. |

## 8. Checklist sau rehearsal

- [ ] Đã rehearsal toàn bộ câu chuyện và biết điểm nào có thể đào sâu hoặc bỏ qua.
- [ ] Không có đoạn đổi cửa sổ quá 20 giây.
- [ ] Mỗi sơ đồ có một câu kết luận.
- [ ] Phân biệt “đã triển khai”, “có evidence”, “planned”.
- [ ] Số Batch đọc đúng và có link raw.
- [ ] Không trình bày CSV trend sai dataset.
- [ ] N+1 không có milliseconds giả.
- [ ] Index vẫn placeholder.
- [ ] Giải thích đúng Outbox không phải exactly-once.
- [ ] Trả lời được Worker chết giữa chunk và broker lỗi sau DB commit.
- [ ] Không lộ secret trong terminal, DB client hoặc Postman.

## 9. Câu kiểm tra cuối trước khi lên sân khấu

```text
Dự án giải quyết gì?
Tại sao chọn kiến trúc này?
Failure ở mỗi boundary được xử lý thế nào?
Em đo bằng gì và evidence nào thực sự hợp lệ?
Trade-off và phần chưa hoàn thiện là gì?
```

Nếu trả lời rõ năm câu này, phần demo sẽ không biến thành một tour màn hình.
