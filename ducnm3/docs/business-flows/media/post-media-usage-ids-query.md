# Truy vấn media usage ID theo owner

API contract: [`POST media usage IDs query`](../../api/media-service/endpoints/post-media-usage-ids-query.md).

Course Service gửi danh sách owner scope để snapshot các usage active trước khi xóa aggregate. Media Service chỉ đọc `media_usages` có `deleted_at IS NULL`, loại trùng ID rồi trả response envelope. Query không dùng RabbitMQ và không thay đổi Media object.
