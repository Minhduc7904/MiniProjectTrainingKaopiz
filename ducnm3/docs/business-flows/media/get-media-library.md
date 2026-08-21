# Liệt kê Media Library

API contract: [`get-media-library.md`](../../api/media-service/endpoints/get-media-library.md)

## Mục tiêu

Cho actor chọn media original đã upload để gán usage hoặc chèn vào Markdown.

## Luồng chính

1. Client gửi `GET /media/api/media/library` kèm actor header và filter tùy chọn.
2. Media Service lọc ownership của actor, media chưa xóa và chỉ nhận record
   original (`sourceMediaId = null`).
3. Service sắp xếp `createdAtUtc DESC, id DESC`, áp dụng cursor và trả item.
4. UI hiển thị `thumbnailUrl` nếu thumbnail derivative đã `READY`; nếu không
   dùng `contentUrl` của original để preview.
5. Khi người dùng chọn, client luôn truyền `item.id` (original) vào media usage
   hoặc dùng `item.contentUrl` (original) để chèn Markdown.

## Trường hợp rỗng và lỗi

- Không có media khớp: `200` với `items: []`.
- Filter, cursor hoặc kích thước trang không hợp lệ: `400 INVALID_MEDIA`.

## Dữ liệu thay đổi

Không có; đây là luồng chỉ đọc. Thumbnail derivative chỉ là dữ liệu preview và
không bao giờ được liệt kê như một item có thể chọn.
