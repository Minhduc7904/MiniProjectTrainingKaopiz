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
| Input / Output | Multipart file và metadata / media gốc, content URL, trạng thái thumbnail. |
| Main flow | Validate actor/file → tạo metadata `PENDING` → stream object → `READY` → enqueue thumbnail nếu hỗ trợ. |
| Error cases | File/actor sai; vượt giới hạn; Student/MinIO không sẵn sàng; thumbnail thất bại. |
| AC | Given file hợp lệ, when upload thành công, then media gốc `READY` và response không lộ storage key; when thumbnail lỗi, then media gốc vẫn truy cập được và có thể retry. |
| Cases | Happy: upload ảnh. Boundary: kích thước tối đa, loại cần thumbnail. Negative: MIME giả, quá cỡ, actor không tồn tại, storage lỗi. |

## F10 — Truy cập Media an toàn

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Client lấy metadata, nội dung, thumbnail hoặc URL usage được phép. |
| Preconditions | Media/usage tồn tại, chưa bị xóa và caller có quyền cần thiết. |
| Input / Output | Media ID hoặc owner/usage query / metadata, stream hoặc URL an toàn. |
| Main flow | Validate → kiểm tra trạng thái/quyền → lấy dữ liệu hoặc stream qua Media Service. |
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
| Error cases | Tuple sai, actor/owner/media không tồn tại, media chưa READY, duplicate active reference. |
| AC | Given avatar mới hợp lệ, when thay avatar, then usage active cũ bị soft-delete và chỉ còn một usage active; when worker redelivery command Notification, then không có usage trùng. |
| Cases | Happy: A → B → A. Boundary: owner chunk 500 và 1,000 usage rows. Negative: gửi lại A khi đang active, media không READY, actor không có quyền. |
