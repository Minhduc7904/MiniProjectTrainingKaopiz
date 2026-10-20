# Assumption register

| ID | Assumption | Lý do / evidence | Impact nếu sai | Owner | Status |
| --- | --- | --- | --- | --- | --- |
| A-01 | Benchmark chạy local chỉ để so sánh relative Before/After. | Không có production environment; `docs/development/performance.md`. | Không dùng số liệu để kết luận production capacity. | Developer/Lead | Đang áp dụng |
| A-02 | Docker Engine khả dụng khi chạy Testcontainers/Compose. | Integration test và stack phụ thuộc container; `docker-compose.yml`. | Cần environment thay thế trước Verify. | Developer | Đang áp dụng |
| A-03 | Lead chưa quy định performance threshold tuyệt đối. | Requirements chỉ khóa dataset và mục tiêu so sánh. | Ghi số đo và xin target trước acceptance. | Lead | Cần xác nhận trước G3 |
| A-04 | Scheduler execution vẫn Planned. | Source chỉ có host/schema/health evidence. | Không đưa vào demo như capability hoàn thành. | Developer/Lead | Đang áp dụng |
