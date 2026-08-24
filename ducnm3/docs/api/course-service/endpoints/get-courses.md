# `GET /course/api/courses`

## Mục đích

Course Service trả danh sách Course theo offset pagination. Direct path là `GET /api/courses`; endpoint chỉ đọc, safe, idempotent và không có request body. Xem [luồng nghiệp vụ](../../../business-flows/courses/get-courses.md).

## Yêu cầu

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `search` | string | Không có | Optional; trim. Tìm khớp một phần `name`, không phân biệt hoa/thường theo collation database. Chuỗi rỗng không lọc. |
| `status` | string | Không có | `DRAFT`, `PUBLISHED` hoặc `ARCHIVED`; trim và uppercase. |
| `sortBy` | string | `createdAt` | Allowlist: `createdAt`, `name`. |
| `sortDirection` | string | `desc` | `asc` hoặc `desc`. |
| `page` | integer | `1` | Từ `1`. |
| `pageSize` | integer | `20` | Từ `1` đến `100`. |

Các filter kết hợp theo `AND`. Ví dụ: `GET /course/api/courses?search=backend&status=PUBLISHED&page=1&pageSize=20`.
List rỗng vẫn là `200`.

## Phản hồi thành công

```json
{
  "data": [{ "id": "course-uuid", "name": "Backend Fundamentals", "status": "PUBLISHED", "createdAtUtc": "2026-08-19T01:00:00Z" }],
  "meta": { "traceId": "01J...", "pagination": { "type": "offset", "page": 1, "pageSize": 20, "totalItems": 1, "totalPages": 1 } }
}
```

## Sắp xếp và hiệu năng

Total order là `created_at <direction>, id <direction>` hoặc `name <direction>, id <direction>`. `id` là tie-breaker duy nhất nên bản ghi trùng sort key không bị đảo thứ tự trong cùng snapshot dữ liệu. Offset có thể dịch chuyển khi Course được thêm/xóa giữa hai request. Repository có `GetPagedAsync` cho API và `GetAllAsync` chỉ phục vụ benchmark/CSV streaming sau này; endpoint không materialize toàn bộ Course.

`search` dùng substring match trên `courses.name`; vì vậy index B-tree hiện có không tăng tốc các từ khóa ở giữa tên. API vẫn giới hạn page tối đa 100 và không thêm migration/index chỉ cho filter này.

## Mã trạng thái và cache

- `200`: kể cả trang rỗng.
- `400 VALIDATION_FAILED`: filter, sort, direction, page hoặc pageSize không hợp lệ.

Response đặt `Cache-Control: no-store` do danh sách thay đổi theo dữ liệu Course.
