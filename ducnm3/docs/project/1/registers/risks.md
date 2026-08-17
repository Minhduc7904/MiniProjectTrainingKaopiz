# Risk register

| ID | Risk | Probability | Severity | Tác động | Giảm thiểu | Owner | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| R-01 | Scope LMS/performance mở rộng vượt mini-project. | Trung bình | Cao | Trễ milestone. | Giữ Out of Scope và review trước feature mới. | Developer/Lead | Open |
| R-02 | Benchmark không tái lập. | Trung bình | Cao | Demo thiếu tin cậy. | Ghi machine, dataset, config, warm-up và số lần chạy. | Developer | Open |
| R-03 | Broker/storage/container local không sẵn sàng. | Trung bình | Cao | Test/demo bị chặn. | Health check, runbook Docker và kiểm tra sớm. | Developer | Open |
| R-04 | Docs khẳng định Planned capability là runtime. | Trung bình | Cao | Review/demo sai kỳ vọng. | Dùng nhãn `Current`/`Planned` và source evidence. | Developer | Open |
| R-05 | Batch/CSV materialize toàn bộ dataset gây peak memory cao hoặc OOM. | Cao | Cao | Benchmark thất bại, API/Worker không scale. | Giữ baseline để so sánh, sau đó page/chunk/stream và đo peak memory. | Developer | Open |
| R-06 | Thêm index theo cảm tính làm tăng write/storage nhưng query không cải thiện. | Trung bình | Trung bình | Kết luận optimization sai. | Dùng 10k/100k/1M và `EXPLAIN ANALYZE` trước/sau; ghi độ chọn lọc. | Developer | Open |
| R-07 | Retry/redelivery tạo notification hoặc media usage trùng. | Trung bình | Cao | Sai dữ liệu nghiệp vụ và counter batch. | Snapshot unique, consumer idempotent, retry đúng một lần và negative test. | Developer | Open |
