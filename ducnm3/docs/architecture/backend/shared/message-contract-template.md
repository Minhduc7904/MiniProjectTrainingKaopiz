# Template message contract

Chỉ tạo message khi có use case thật; contract đặt tại service owner và không dùng entity database làm payload.

| Trường | Nội dung |
| --- | --- |
| Name / version | Tên ổn định, ví dụ V1 |
| Kind | COMMAND hoặc EVENT |
| Owner | Service sở hữu contract |
| Producer / consumer | Một owner cho COMMAND; subscriber cho EVENT |
| Idempotency | Business key hoặc message ID |
| Failure | Retry tập trung và error queue |

COMMAND dùng Send, EVENT dùng Publish. Breaking change tạo version mới. Luồng ghi database và publish message quan trọng cần Outbox/Inbox.

