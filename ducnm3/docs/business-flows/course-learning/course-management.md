# Course and Lesson Management

## Mục đích

Admin tạo, cập nhật, xuất bản, và quản lý Lesson cho một Course.

## Actor

Admin.

## Điều kiện đầu vào

- Admin đã được xác thực và có quyền quản lý Course.
- `name` hợp lệ; `descriptionMarkdown` là Markdown an toàn.

## Luồng chính

1. Admin gửi `POST /api/courses` với tên, mô tả Markdown, và trạng thái `DRAFT`.
2. Course Service validate dữ liệu và lưu một record `courses`.
3. Admin thêm Lesson bằng `POST /api/courses/{courseId}/lessons`.
4. Course Service kiểm tra Course tồn tại, lưu `lessons` với `display_order`.
5. Admin cập nhật Course, Lesson, hoặc chuyển Course sang `PUBLISHED`.
6. Student chỉ thấy Course và Lesson đã được phép truy cập theo trạng thái và enrollment.

## Trường hợp lỗi

- `400`: tên, Markdown, hoặc `display_order` không hợp lệ.
- `404`: Course không tồn tại.
- `409`: Lesson có `display_order` trùng trong cùng Course hoặc Course không thể xuất bản.

## Dữ liệu thay đổi

- Course Service database: `courses`, `lessons`.
- Không có service nào khác bị ghi dữ liệu trong luồng này.

## Liên quan

- [Media upload and usage](../media/media-upload-and-usage.md) cho thumbnail, attachment, và media nhúng.
- [Enrollment and learning progress](enrollment-and-learning-progress.md) cho quyền học của Student.
