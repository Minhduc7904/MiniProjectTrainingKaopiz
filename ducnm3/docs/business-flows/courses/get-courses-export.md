# Xuất CSV Course

## Mục đích

Client tải danh sách Course lớn để phân tích hoặc lưu trữ mà không buộc Course
Service phải nạp toàn bộ dữ liệu xuất vào memory.

## Tác nhân và đầu vào

- Client có quyền truy cập Course Service qua Gateway.
- `status` tùy chọn phải là `DRAFT`, `PUBLISHED` hoặc `ARCHIVED` sau normalize.

## Luồng chính

1. Client gửi `GET /course/api/courses/export` không body.
2. API validate filter trước khi gửi CSV headers.
3. API ghi UTF-8 BOM và CSV header để Excel đọc tiếng Việt đúng.
4. Repository đọc Course `AsNoTracking` theo `createdAtUtc DESC, id DESC`, tối đa
   500 rows mỗi chunk.
5. API escape từng field CSV và ghi ngay row vào response body; tiếp tục với
   keyset position cuối chunk đến khi chunk ngắn hơn 500 rows.
6. Client nhận file `courses.csv`.

## Lỗi và hủy

- `status` sai: `400 VALIDATION_FAILED` JSON, chưa ghi CSV body.
- Client đóng kết nối: cancellation token đi xuyên response write và database
  query, không có side effect cần rollback.

## Tính nhất quán và dữ liệu thay đổi

Export gồm nhiều query read-only nên không là database snapshot. Ghi mới/xóa/sửa
trong lúc export có thể làm tập kết quả thay đổi; keyset ngăn row đã ghi lặp lại.
Không thay đổi dữ liệu Course, không publish message.
