# Liệt kê Media Library

API contract: [`get-media-library.md`](../../api/media-service/endpoints/get-media-library.md)

## Mục tiêu

Cho actor chọn media original đã upload để gán usage hoặc chèn vào Markdown.

## Luồng chính

1. Client gửi `GET /media/api/media/library` kèm actor header và filter tùy chọn.
2. Media Service lọc ownership của actor, media chưa xóa và chỉ nhận record
   original (`sourceMediaId = null`). Nếu client gửi `status`, service chỉ trả
   trạng thái `PENDING`, `READY` hoặc `FAILED` tương ứng.
3. Service sắp xếp `createdAtUtc DESC, id DESC`, áp dụng cursor và trả item.
4. Với ảnh, video và PDF, danh sách Grid/List dùng `thumbnail.contentUrl` (hoặc
   field tương thích ngược `thumbnailUrl`) khi derivative đã `READY`. Thumbnail
   `QUEUED`, `PROCESSING` hoặc `FAILED` hiển thị trạng thái thay vì tải file
   original để dựng cover. Audio, Other và document không phải PDF là
   `NOT_REQUIRED`.
5. Khi người dùng chọn item `READY`, panel chi tiết xem `item.contentUrl` của
   file original; thumbnail chỉ còn dùng cho cover danh sách và metadata. Client
   truyền `item.id` (original) vào media usage hoặc dùng `item.contentUrl`
   (original) để chèn Markdown.

## Trường hợp rỗng và lỗi

- Không có media khớp: `200` với `items: []`.
- Filter `mediaType`/`status`, cursor hoặc kích thước trang không hợp lệ:
  `400 INVALID_MEDIA`.

## Dữ liệu thay đổi

Không có; đây là luồng chỉ đọc. Thumbnail derivative chỉ là dữ liệu preview và
không bao giờ được liệt kê như một item có thể chọn.
