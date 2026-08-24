# Bộ tài liệu thuyết trình Mini LMS

## Mục đích

Thư mục này là nguồn chuẩn để chuẩn bị buổi demo cuối kỳ. Nội dung bám theo
source, tài liệu trong `docs/development/` và evidence hiện có trong
`performance/results/`; không biến kế hoạch hoặc dữ liệu chưa kiểm chứng thành
kết quả đã hoàn thành.

## Thứ tự sử dụng

1. [Kịch bản thuyết trình](01-kich-ban-thuyet-trinh.md): lời nói, thao tác demo
   và câu chuyển ý theo đúng thứ tự trình bày.
2. [Kiến trúc, Worker và debug](02-kien-truc-worker-debug.md): phần đào sâu để
   thuyết trình kiến trúc và trả lời câu hỏi kỹ thuật.
3. [Performance và evidence](03-performance-evidence.md): cách chạy tool, ý
   nghĩa metric, kết quả hiện có và các điểm chưa đủ điều kiện công bố.
4. [Checklist demo](04-checklist-demo.md): chuẩn bị môi trường, dữ liệu, cửa sổ
   cần mở, phương án dự phòng và checklist trước giờ trình bày.
5. [Giải thích Outbox và Inbox](05-outbox-inbox-notes.md): note chuyên sâu về
   Bus Outbox, Consumer Outbox, ý nghĩa từng cột và cách đọc ba bảng MassTransit.
6. [Phao RabbitMQ, at-least-once, dual-write, Outbox và Inbox](06-phao-rabbitmq-outbox-inbox.md):
   bản ôn nhanh, câu trả lời khi bị hỏi và quy trình debug bằng console.
7. [Phao Docker Compose và MinIO](07-phao-docker-minio.md): runtime inventory,
   startup dependency, năm bucket, object-key convention, upload flow và câu hỏi
   phản biện.
8. [Phao Lease Token](08-phao-lease-token.md): worker nào claim, SQL
   `FOR UPDATE SKIP LOCKED`, token/expiry, fencing và giới hạn recovery hiện tại.
9. [Bộ luồng HTML tương tác](interactive/README.md): Docker, MinIO,
   Outbox/Inbox, Batch Notification và CSV streaming.

## Cách triển khai long-form

Kịch bản không có hard-stop thời lượng. Nội dung được chia thành các module theo
đúng câu chuyện của hệ thống:

1. Dự án và demo FE.
2. Quy trình làm việc và cách tạo Skill.
3. Kiến trúc, Docker runtime và MinIO storage design.
4. Worker, RabbitMQ, Outbox/Inbox và debug console.
5. Ba bài toán Batch Notification, CSV và Media.
6. Performance, N+1, Index roadmap và kết luận.

Có thể mở code, Compose, MinIO Console, RabbitMQ Management và database ở từng
module. Phần SQL/code chi tiết là nhánh đào sâu, không phải checklist bắt buộc.
Index vẫn chỉ là roadmap vì chưa có evidence trước/sau.

## Trạng thái evidence trước khi lên slide

| Nội dung | Trạng thái | Cách trình bày |
| --- | --- | --- |
| Batch 3k/10k/100k | Có raw JSON, 3 run/dataset | Có thể trình bày số liệu với điều kiện ghi rõ môi trường còn thiếu trong metadata. |
| CSV buffered/streaming | Có raw JSON nhưng dataset/row count không khớp | Chỉ dùng để giải thích metric và phát hiện lỗi evidence; chạy lại trước khi kết luận. |
| N+1 | Có hai query path trong source, chưa có kết quả benchmark | Demo query shape; không đưa response-time giả. |
| Index | Tạm để trống theo yêu cầu | Dùng slide placeholder, không claim trước/sau. |
| Serilog console | Đã có structured console log | Demo bằng `docker compose logs`, Correlation ID hoặc batch ID. |

> [!IMPORTANT]
> `docs/development/performance.md` yêu cầu không ghi số giả lên slide. Mọi số
> liệu dùng trong buổi nói phải truy ngược được về raw result và cùng một cấu
> hình dataset/môi trường.

## Tài liệu nguồn chính

- [Performance requirements](../development/performance.md)
- [Performance runner guide](../performance/README.md)
- [Backend architecture](../architecture/backend/overview.md)
- [Frontend architecture](../architecture/frontend/architecture.md)
- [Notification batch flow](../business-flows/notifications/bulk-notification.md)
- [CSV export flow](../business-flows/courses/get-courses-export.md)
- [Media development guide](../development/minio.md)
- [Observability guide](../development/observability.md)
- [Developer task guide](../guide/DEV_TASK_GUIDE.md)
- [DevKit guide](../guide/DEVKIT_GUIDE.md)
