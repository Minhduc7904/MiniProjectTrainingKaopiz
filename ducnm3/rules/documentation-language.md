# Quy tắc ngôn ngữ tài liệu

- Viết phần diễn giải, hướng dẫn, tiêu đề và chú thích trong `docs/**/*.md` bằng
  tiếng Việt rõ ràng, tự nhiên.
- Không dịch thuần Việt các thuật ngữ kỹ thuật đã phổ biến trong đội phát triển.
  Có thể giữ nguyên: `request`, `response`, `payload`, `endpoint`, `API`,
  `service`, `database`, `migration`, `seed`, `batch`, `retry`, `worker`,
  `middleware`, `repository`, `adapter`, `stream`, `cache`, `metadata`,
  `container`, `bucket`, `commit`, `merge`, `pull request` và các thuật ngữ
  tương tự.
- Ưu tiên câu tiếng Việt có thuật ngữ tiếng Anh đúng ngữ cảnh, thay vì dịch từng
  từ làm câu khó hiểu.
- Khi thuật ngữ ít quen thuộc xuất hiện lần đầu, có thể giải thích ngắn bằng
  tiếng Việt trong ngoặc; các lần sau giữ nguyên thuật ngữ.
- Giữ nguyên tên class, method, project, service, table, column, field, enum,
  error code, HTTP method/status, API path, environment variable, command, URL
  và nội dung code block.
- Không dịch nội dung nếu việc dịch làm thay đổi API contract, schema, ví dụ dữ
  liệu, log/error chính xác hoặc hành vi kỹ thuật.
- Dùng thuật ngữ nhất quán trong cùng tài liệu và giữa các tài liệu liên quan.

Ví dụ nên viết:

- `Request payload được validate trước khi Application xử lý.`
- `Response thành công sử dụng response envelope dùng chung.`
- `Worker retry batch bị lỗi theo idempotency key.`

Tránh cách viết dịch thuần Việt gây khó hiểu:

- `Tải trọng yêu cầu được kiểm tra tính hợp lệ trước khi tầng ứng dụng xử lý.`
- `Bộ lao động thử lại lô theo khóa chống trùng lặp.`
