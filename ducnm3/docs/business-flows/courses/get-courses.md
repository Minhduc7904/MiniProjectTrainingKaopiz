# Liệt kê Course có phân trang

## Mục đích

Người dùng hoặc client lấy danh sách Course để hiển thị theo trang mà không tải toàn bộ dữ liệu.

## Luồng chính

1. Client gửi `GET /course/api/courses` với `search`, filter/sort/page hợp lệ và không gửi body.
2. API bind query, Application normalize/validate allowlist.
3. Repository trim `search`, tìm một phần tên Course (nếu có), áp dụng `status`, stable order và `Skip`/`Take` sau khi đếm cùng predicate.
4. API trả `200` response envelope cùng `meta.pagination` offset và `Cache-Control: no-store`.
5. Client chọn page tiếp theo theo `totalPages`.

## Trường hợp rỗng và lỗi

- Không có Course phù hợp với `search` hoặc filter: `200`, `data: []`, metadata total bằng `0`.
- Query không hợp lệ: `400 VALIDATION_FAILED`; repository không được gọi.

## Tính nhất quán và hiệu năng

Offset page có thể dịch chuyển khi dữ liệu thay đổi giữa các request. API chỉ dùng `GetPagedAsync`; `GetAllAsync` là đường repository tách biệt để benchmark chênh lệch memory/time và làm baseline cho CSV streaming, không được gọi qua endpoint này. Tìm kiếm là substring trên tên Course; từ khóa không ở đầu tên có thể không dùng được B-tree index.
