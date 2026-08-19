# Course CSV Export Design

## Mục tiêu

Triển khai Course Service endpoint `GET /api/courses/export` để tải danh sách
Course dạng CSV lớn mà không materialize toàn bộ dataset hoặc nội dung CSV trong
bộ nhớ. File mở đúng tiếng Việt trong Microsoft Excel.

## Contract

- Query duy nhất: `status` tùy chọn, trim/uppercase, chỉ nhận `DRAFT`,
  `PUBLISHED`, `ARCHIVED`.
- Thành công trả `200 text/csv; charset=utf-8`, attachment filename
  `courses.csv`, UTF-8 BOM và header `id,name,status,createdAtUtc`.
- CSV theo RFC 4180: field chứa dấu phẩy, dấu quote, CR hoặc LF được bọc quote;
  quote trong giá trị được nhân đôi.
- Query sai trả standard JSON `400 VALIDATION_FAILED`; sau khi streaming bắt đầu
  không thay đổi status/error body được nữa.

## Kiến trúc và dữ liệu

`Api -> Application <- Infrastructure` được giữ nguyên. API chỉ set header và
ghi từng row; Application điều phối export/query và CSV encoder; Infrastructure
đọc projection `id,name,status,created_at` bằng `AsNoTracking` theo keyset
`created_at DESC, id DESC` từng chunk cố định 500 rows. `status` được áp dụng
trước keyset predicate. Mỗi lần chỉ có một chunk và một row CSV tồn tại trong
memory.

Repository `GetAllAsync` vẫn giữ riêng cho benchmark baseline. Export dùng thêm
`ReadExportChunkAsync` và không được gọi `GetAllAsync`/`GetPagedAsync`.

## Cancellation và nhất quán

`HttpContext.RequestAborted` được truyền xuyên API, handler, repository và
`Response.Body.WriteAsync`; client cancel dừng query/chunk kế tiếp. Vì các
chunk là nhiều query read-only, insert/delete trong lúc export có thể làm output
không phải snapshot transaction; keyset ngăn record đã đọc xuất lại. Không có
side effect dữ liệu.

## Kiểm thử

- Unit: CSV escaping, BOM/header và handler chỉ gọi stream path.
- Component: CSV headers/body với TestServer, status invalid trả JSON standard,
  cancellation token được truyền.
- Integration MySQL/Testcontainers cho stable keyset và filter được để cùng
  ticket khi Course IntegrationTests được tạo; task hiện không tạo migration
  hoặc index mới vì index `(status, created_at DESC)` đã tồn tại.

