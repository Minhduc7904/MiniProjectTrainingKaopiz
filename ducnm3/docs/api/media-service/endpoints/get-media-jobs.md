# `GET /media/api/media/jobs`

## Mục đích

Liệt kê các background job do Media Service quản lý để ADMIN theo dõi tiến độ và lỗi vận hành. Direct path là `GET /api/media/jobs`.

Business flow: [`get-media-jobs.md`](../../../business-flows/media/get-media-jobs.md).

## Xác thực và phân quyền

- Bắt buộc `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID của ADMIN tồn tại.
- Actor khác ADMIN nhận `400 INVALID_ACTOR_TYPE` và không đọc được job.

## Query

Không có request body. Các filter kết hợp theo `AND`.

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `jobType` | string | Không có | `THUMBNAIL_DERIVATION`, `MARKDOWN_USAGE_SYNC`, `MEDIA_USAGE_DELETE`, `NOTIFICATION_USAGE`; không phân biệt hoa thường. |
| `status` | string | Không có | `QUEUED`, `PROCESSING`, `COMPLETED`, `PARTIAL_FAILED`, `FAILED`; không phân biệt hoa thường. |
| `correlationId` | UUID | Không có | Correlation logical reference, ví dụ Notification Batch ID. |
| `page` | integer | `1` | Từ `1`. |
| `pageSize` | integer | `20` | Từ `1` đến `100`. |

Thứ tự cố định là `updatedAtUtc DESC, id DESC`; không hỗ trợ sort do client chọn.

## Phản hồi thành công

`200 OK` trả item gồm ID, job type, subject, correlation, status, các counter, remaining/progress, attempt count, lỗi an toàn và timestamps; response dùng `meta.pagination` offset. `payloadJson` và `deduplicationKey` không được trả vì là metadata nội bộ.

## Mã trạng thái và side effects

- `200`: kể cả khi không có job (`data: []`).
- `400 INVALID_MEDIA_JOB_QUERY`: filter, UUID hoặc pagination không hợp lệ.
- `400 INVALID_ACTOR_TYPE`, `404 ACTOR_NOT_FOUND`: actor không đủ quyền hoặc không tồn tại.

Endpoint safe, idempotent, chỉ đọc, `Cache-Control: no-store`. Offset pages có thể dịch chuyển khi worker cập nhật job giữa hai request.
