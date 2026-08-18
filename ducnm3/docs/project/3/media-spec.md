# Requirements — Media

## Quy tắc chung

- `BR-MEDIA-01`: Chỉ Media Service sở hữu metadata media và truy cập MinIO.
- `BR-MEDIA-02`: Bucket/object key không lộ ra response cho client.
- `BR-MEDIA-03`: Actor và owner là hai khái niệm độc lập; chỉ tuple usage được
  hỗ trợ mới hợp lệ.
- `BR-MEDIA-04`: Thumbnail lỗi không làm media gốc đã upload thành công thất bại;
  retry phải không tạo usage trùng.

## F09 — Upload và xử lý Media

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Client upload media; hệ thống lưu object/metadata và tạo thumbnail khi phù hợp. |
| Preconditions | Actor tồn tại; MIME, extension, kích thước và loại media hợp lệ. |
| Input / Output | Multipart file hoặc direct-upload metadata/SHA-256 / draft media, signed POST ngắn hạn, content URL, trạng thái thumbnail. |
| Main flow | Multipart baseline hoặc direct: checksum → intent draft `PENDING` → browser POST MinIO → verify/promote → `READY` draft → enqueue thumbnail nếu hỗ trợ. |
| Alternative flow | Loại media không cần thumbnail kết thúc khi media gốc `READY`; nếu thumbnail thất bại thì media gốc vẫn truy cập được và thumbnail có thể retry. |
| Error cases | File/actor sai; vượt giới hạn; Student/MinIO không sẵn sàng; upload media gốc thất bại. |
| AC | Given file hợp lệ, when multipart/direct upload thành công, then media gốc `READY`, `isDraft=true` và response thường không lộ storage key; signed intent hết hạn 15 phút và không được log. Reload direct page không restore transfer. |
| Cases | Happy: multipart/direct ảnh. Boundary: 500 MiB, policy 15 phút, concurrent complete. Negative: MIME giả, quá cỡ, actor không tồn tại, CORS/storage lỗi, checksum metadata/ETag stale. |

Direct-upload checksum là metadata SHA-256 frontend khai báo và được signed
policy ràng buộc; finalize không đọc toàn bộ bytes để tự băm. Complete retry
idempotent; intent creation không idempotent. P5-13 sở hữu draft transition theo
usage và P5-20 sở hữu cleanup có reference recheck.

## F10 — Truy cập Media an toàn

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Client lấy metadata, nội dung, thumbnail hoặc URL usage được phép. |
| Preconditions | Media/usage tồn tại, chưa bị xóa và caller có quyền cần thiết. |
| Input / Output | Media ID hoặc owner/usage query / metadata, stream hoặc URL an toàn. |
| Main flow | Validate → kiểm tra trạng thái/quyền → lấy dữ liệu hoặc stream qua Media Service. |
| Alternative flow | Caller có thể truy cập nội dung gốc, thumbnail `READY` hoặc URL theo usage; thumbnail chưa sẵn sàng không làm mất quyền truy cập media gốc. |
| Error cases | Không tồn tại, đã xóa/chưa sẵn sàng, không có quyền, storage không sẵn sàng. |
| AC | Given media READY hợp lệ, when caller được phép truy cập, then nhận nội dung/URL; when media không tồn tại hoặc không được phép, then không lộ object location. |
| Cases | Happy: nội dung và thumbnail READY. Boundary: thumbnail `QUEUED`/`FAILED`. Negative: ID sai, usage bị soft-delete, storage lỗi. |

## F11 — Quản lý Media usage

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Gắn media READY với avatar, thumbnail hoặc nội dung Notification; duy trì history an toàn. |
| Preconditions | Actor/owner hợp lệ; media READY; tuple owner/usage được hỗ trợ. |
| Input / Output | Media ID, actor, owner, usage type / usage mới hoặc conflict. |
| Main flow | Validate → xác minh actor/owner → thay usage active trong transaction hoặc tạo usage idempotent từ worker. |
| Alternative flow | Thay avatar soft-delete usage active cũ trước khi tạo usage mới; redelivery command hợp lệ trả kết quả idempotent và không tạo usage trùng. |
| Error cases | Tuple sai, actor/owner/media không tồn tại, media chưa READY, duplicate active reference. |
| AC | Given avatar mới hợp lệ, when thay avatar, then usage active cũ bị soft-delete và chỉ còn một usage active; when worker redelivery command Notification, then không có usage trùng. |
| Cases | Happy: A → B → A. Boundary: owner chunk 500 và 1,000 usage rows. Negative: gửi lại A khi đang active, media không READY, actor không có quyền. |
