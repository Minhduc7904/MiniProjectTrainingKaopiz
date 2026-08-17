# Q&A và Design Decisions

## Quy ước

- `Critical`: phải đóng trước G3.
- `Non-critical`: có thể chuyển thành assumption được Lead chấp nhận.
- Khi câu trả lời thay đổi requirement, cập nhật Phase 3 và xin lại G2 trước khi
  cập nhật design/testcase.

## Câu hỏi mở

| ID | Mức | Câu hỏi/ảnh hưởng | Đề xuất | Owner | Trạng thái |
| --- | --- | --- | --- | --- | --- |
| Q4-01 | Critical | Lead đã sign-off G2 cho Phase 3 chưa? Không có G2 thì không thể approve G3. | Review Function List, Alternative Flow, NFR và 13/13 traceability trước. | Lead | Open |
| Q4-02 | Critical | F04 duplicate enrollment hiện Phase 3 yêu cầu idempotent, trong khi business flow cũ mô tả `409`. Success status và `Location` của replay cần thống nhất. | Dùng `Idempotency-Key`; first/replay trả cùng logical Enrollment và canonical `Location`, không tạo row thứ hai. | Lead/BE | Open |
| Q4-03 | Critical | Identity/role lấy từ JWT hay trusted development header? Nhiều contract ghi Admin/Học viên nhưng runtime auth chưa được chứng minh. | Chốt một authentication boundary; không nhận `studentId`/admin role từ payload không đáng tin cậy. | Lead/BE | Open |
| Q4-04 | Critical | Scheduler dùng Outbox và terminal event nào để theo dõi Media cleanup? Nếu thêm MassTransit table sẽ cần migration. | Dùng `CleanupStaleMediaV1` + completed/failed event và Scheduler Outbox/Inbox. | Lead/BE | Open |
| Q4-05 | Non-critical | Course/Lesson PATCH có cần optimistic concurrency không? Schema hiện chưa có version. | MiniProject dùng last accepted write và DB constraints; nếu yêu cầu `If-Match`, bổ sung version + migration trước G3. | Lead | Open |

## Quyết định đã khóa trong draft

| ID | Quyết định | Căn cứ |
| --- | --- | --- |
| D4-01 | Phase 4 index/link source of truth, không copy toàn bộ API/architecture/database docs. | Tránh hai contract lệch nhau. |
| D4-02 | F05 dùng PUT vào URI progress xác định bởi Course/Lesson/current Student; cùng value là idempotent. | Progress là representation thay thế đầy đủ bằng `progressPercent`. |
| D4-03 | F15–F16 và F18 không mở public worker endpoint. | Background processing qua command; trạng thái đọc qua resource API khi có. |
| D4-04 | Functional testcase tách theo domain; NFR testcase tách riêng và bắt buộc có dataset/metrics. | Phân biệt business correctness và performance/quality evidence. |

## Cách đóng Q&A

Ghi câu trả lời, người xác nhận và ngày xác nhận ngay trong bảng; sau đó cập nhật
đồng thời API/database/messaging matrix, testcase và traceability. Không chỉ
đánh dấu `Closed` nếu artefact liên quan chưa được đồng bộ.

