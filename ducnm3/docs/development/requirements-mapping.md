# Đối chiếu yêu cầu

| Phần | Bài toán / kỹ thuật | Mục tiêu cần hiểu |
|---|---|---|
| Docker | Tự dockerize application + DB + MinIO, setup bằng docker-compose | Hiểu container, network, volume, environment/config |
| MinIO / Object Storage | Upload/download ảnh; lưu file trên MinIO thay vì local | Hiểu object storage và cách quản lý file độc lập với filesystem của application |
| Batch Job | Gửi notification cho ~3k user theo một điều kiện; xử lý bằng batch/background job, không xử lý toàn bộ trong một request/transaction | Hiểu batch processing, chunking, background job |
| Batch – Retry | Khi gửi lỗi thì retry 1 lần; nếu vẫn fail thì ghi nhận trạng thái/error để trace | Hiểu retry, failure handling, idempotency |
| Batch – Performance | Test với 3k → 10k → 100k user, đo execution time và memory usage | Hiểu khả năng scale của batch, tránh load toàn bộ data lên memory |
| CSV Export | Export danh sách khóa học với 100k+ records | Hiểu streaming/chunking và cách tránh OOM |
| CSV – Performance | So sánh SELECT ALL → build CSV → response với xử lý theo batch/stream | Hiểu trade-off giữa memory và performance |
| API – N+1 | API Course → Lessons → Progress; kiểm tra số lượng SQL query khi lấy nhiều records | Nhận biết và xử lý N+1 |
| API – Index | Test query trên 10k / 100k / 1M records, so sánh trước và sau khi tạo index | Hiểu khi nào index có tác dụng, không phải query nào cũng nên thêm index |
| API – Query Plan | Sử dụng EXPLAIN / EXPLAIN ANALYZE để kiểm tra query | Biết đọc execution plan, index scan/full scan và xác định bottleneck |
| API – Pagination | API list với dataset lớn; thử offset pagination và kiểm tra performance khi offset tăng | Hiểu vấn đề của pagination trên dataset lớn và khi nào cần cách tiếp cận khác |
| API – Performance | Benchmark API với 10k / 100k records, đo response time trước/sau optimization | Biết đo performance bằng số liệu thay vì cảm tính |
| API – Error Handling | Xử lý validation error, resource not found, DB error và trả response phù hợp | Hiểu API design và error handling thực tế |