# Requirements — Notification

## Quy tắc chung

- `BR-NOTI-01`: Thông báo đơn và batch chỉ được tạo bởi actor có quyền; recipient
  phải là Học viên `ACTIVE` tại thời điểm kiểm tra/snapshot.
- `BR-NOTI-02`: Batch không xử lý toàn bộ recipient trong HTTP request hoặc giữ
  toàn bộ recipient trong memory.
- `BR-NOTI-03`: Recipient batch phải được snapshot trước dispatch và unique theo
  `(batch, student)`; item `SUCCESS` không được gửi lại.
- `BR-NOTI-04`: Mỗi item retry tối đa một lần; lần thất bại thứ hai được trace
  thành `FAILED` với thông tin lỗi an toàn.
- `BR-NOTI-05`: Markdown tham chiếu media phải hợp lệ; usage chỉ được tạo cho
  notification đã tạo thành công.

## F12 — Tạo thông báo đơn

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Admin gửi một thông báo đến một Học viên. |
| Preconditions | Admin có quyền; recipient tồn tại/ACTIVE; title và Markdown hợp lệ. |
| Input / Output | Recipient, title, body, người tạo / notification `UNREAD` hoặc lỗi. |
| Main flow | Validate → tạo notification đơn → durable handoff Media usage nếu có → trả kết quả. |
| Alternative flow | Nội dung không tham chiếu media kết thúc sau khi tạo notification; nội dung có media hợp lệ thực hiện durable handoff để đăng ký usage. |
| Error cases | Không có quyền; recipient không tồn tại/inactive; Markdown/media sai. |
| AC | Given recipient ACTIVE và payload hợp lệ, when Admin tạo thông báo, then inbox có một item `UNREAD`; when Markdown media sai, then không tạo notification hoặc usage dang dở. |
| Cases | Happy: thông báo không/có media. Boundary: title/body giới hạn. Negative: recipient sai, media không READY, không có quyền. |

## F13 — Quản lý hộp thư đến

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Học viên xem, đọc chi tiết và đánh dấu đã đọc thông báo của chính mình. |
| Preconditions | Học viên xác thực. |
| Input / Output | Danh tính hiện tại và notification ID khi cần / inbox hoặc trạng thái đọc. |
| Main flow | Resolve identity → lọc inbox theo recipient → kiểm tra ownership → chuyển `UNREAD` sang `READ`. |
| Alternative flow | Inbox không có item trả danh sách rỗng; đánh dấu lại item đã `READ` hoặc read-all khi không còn item `UNREAD` không tạo thay đổi trùng. |
| Error cases | Chưa xác thực; notification không tồn tại; truy cập notification của người khác. |
| AC | Given notification thuộc Học viên, when đánh dấu đọc, then trạng thái và `read_at` được cập nhật; when Học viên khác thao tác, then dữ liệu không đổi. |
| Cases | Happy: list/detail/read/read-all. Boundary: inbox rỗng, item đã đọc. Negative: ID sai, ownership sai, identity thiếu. |

## F14 — Tạo Notification batch

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Admin yêu cầu gửi thông báo cho toàn bộ Học viên theo target scope được hỗ trợ. |
| Preconditions | Admin có quyền; `ALL_STUDENTS`, createdBy, title, Markdown hợp lệ; database/outbox sẵn sàng. |
| Input / Output | Scope và nội dung batch / batch `PENDING` và thông tin theo dõi. |
| Main flow | Validate → tạo batch và durable snapshot command → chấp nhận yêu cầu, không gửi inbox đồng bộ. |
| Alternative flow | Batch không tham chiếu media chỉ tạo snapshot command; batch có media hợp lệ lưu cùng nội dung để đăng ký usage sau từng dispatch chunk thành công. |
| Error cases | Scope/payload sai; không có quyền; persistence/outbox không sẵn sàng. |
| AC | Given 3,000 Học viên ACTIVE, when Admin tạo batch hợp lệ, then request được chấp nhận và batch được tạo trước background processing; then HTTP request không tạo toàn bộ recipient/inbox. |
| Cases | Happy: ALL_STUDENTS. Boundary: 1/3k/10k/100k recipient. Negative: scope sai, title rỗng, outbox lỗi. |

## F15 — Snapshot recipient batch

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Notification Worker chốt danh sách recipient của batch theo trang từ Student Service. |
| Preconditions | Batch `PENDING`; snapshot command durable; Student Service sẵn sàng. |
| Input / Output | Batch ID / batch items unique, total count và trạng thái snapshot. |
| Main flow | Claim trạng thái → đọc Student ACTIVE theo trang → upsert item → `SNAPSHOT_READY` hoặc `FAILED` → enqueue dispatch. |
| Alternative flow | Command bị redelivery tiếp tục/upsert snapshot theo khóa `(batch, student)` và không tạo item trùng; trang cuối ít hơn page size vẫn chốt đúng total. |
| Error cases | Batch không tồn tại/trạng thái không hợp lệ; Student Service lỗi; không có recipient. |
| AC | Given Student Service trả nhiều trang ACTIVE, when snapshot, then mỗi student chỉ có một item và total đúng; when recipient rỗng hoặc snapshot lỗi, then batch không dispatch nhầm và có trạng thái traceable. |
| Cases | Happy: nhiều trang. Boundary: 0, 1, 100k recipient và redelivery. Negative: page lỗi, batch sai trạng thái. |

## F16 — Dispatch và retry Notification batch

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Worker gửi item theo chunk, tạo inbox thành công và retry lỗi đúng một lần. |
| Preconditions | Batch `SNAPSHOT_READY` hoặc còn item retry; worker configuration hợp lệ. |
| Input / Output | Batch ID và item claim / notification, trạng thái item, counter và trạng thái batch. |
| Main flow | Claim item theo lease → gửi tối đa concurrency quy định → ghi kết quả theo chunk → enqueue tiếp hoặc kết thúc batch. |
| Alternative flow | Item lỗi lần đầu chuyển sang retry đúng một lần; redelivery hoặc lease stale không xử lý lại item `SUCCESS` và không ghi kết quả bằng token không hợp lệ. |
| Error cases | Lease hết hạn/token không khớp; sender lỗi; worker bị redelivery; media usage command lỗi. |
| AC | Given item gửi lỗi lần đầu, when dispatch, then item được retry một lần; when lỗi lần hai, then item `FAILED` và batch là `PARTIAL_FAILED`/`FAILED` phù hợp; when redelivery, then không tạo inbox/usage trùng. |
| Cases | Happy: tất cả thành công. Boundary: chunk 500, concurrency cấu hình. Negative: sender fail hai lần, lease stale, command lặp. |

## F17 — Theo dõi Notification batch

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Admin/client kiểm tra trạng thái, counter và item lỗi của batch. |
| Preconditions | Batch ID hợp lệ; caller được phép tra cứu. |
| Input / Output | Batch ID và phân trang item lỗi / summary hoặc danh sách lỗi. |
| Main flow | Validate → đọc batch/counter hoặc failed item → trả trạng thái quan sát được. |
| Alternative flow | Batch không có item lỗi trả danh sách rỗng cùng pagination metadata; batch đang xử lý vẫn trả counter và trạng thái hiện tại nhất quán. |
| Error cases | Batch không tồn tại; query không hợp lệ; không có quyền. |
| AC | Given batch đã xử lý, when truy vấn, then trả status và counter nhất quán; when không tồn tại, then không trả batch khác. |
| Cases | Happy: COMPLETED/PARTIAL_FAILED. Boundary: không có failed item. Negative: batchId sai, page lỗi, actor không có quyền. |
