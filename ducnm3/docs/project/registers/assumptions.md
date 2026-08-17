# Assumption register

| ID | Assumption | Lý do | Impact nếu sai |
| --- | --- | --- | --- |
| A-01 | Benchmark chạy local chỉ để so sánh relative Before/After. | Không có production environment. | Không dùng số liệu để kết luận production capacity. |
| A-02 | Docker Engine khả dụng khi chạy Testcontainers/Compose. | Integration test và stack phụ thuộc container. | Cần environment thay thế trước Verify. |
| A-03 | Lead chưa quy định performance threshold tuyệt đối. | Repository chỉ nêu dataset/kế hoạch. | Ghi số đo và xin target trước acceptance. |
| A-04 | Scheduler execution vẫn Planned. | Source chỉ có host/schema/health evidence. | Không đưa vào demo như capability hoàn thành. |
