# Course và Lesson Markdown Update Design

## Mục tiêu

Thêm `POST /api/courses`, `PUT /api/courses/{courseId}` và
`PUT /api/courses/{courseId}/lessons/{lessonId}`. Cập nhật Course hoặc Lesson
chỉ thay đổi các field xuất hiện trong JSON request. Chỉ
`descriptionMarkdown` và `contentMarkdown` kích hoạt xử lý media.

## Contract cập nhật

- `POST /api/courses` tạo Course với `name`, `descriptionMarkdown` và `status`.
  `descriptionMarkdown` là `null` hoặc chuỗi rỗng không tạo media usage.
- Hai endpoint `PUT` dùng partial-update theo yêu cầu sản phẩm dù tên HTTP method
  là PUT. Field không xuất hiện trong JSON không bị thay đổi.
- Một markdown có mặt với giá trị `null` hoặc chuỗi rỗng sẽ được lưu là `null`;
  tất cả media usage thuộc markdown đó được gỡ bất đồng bộ.
- Markdown không rỗng được lưu nguyên văn sau validation hiện có. Response `200`
  trả representation sau cập nhật trong response envelope chuẩn.
- Lesson phải thuộc Course trên route. `title` không rỗng, dài tối đa 200;
  `displayOrder` tối thiểu là 1 và không được trùng trong một Course. Course
  giữ validation `name` dài 3--200 và `status` hợp lệ.
- JSON request không có field nào trả `400 VALIDATION_FAILED` thay vì tạo cập
  nhật rỗng. Course/Lesson không tồn tại trả `404`; trùng `displayOrder` trả
  `409`.

## Đồng bộ media qua worker

Course Service dùng cùng `LessonMediaReferenceExtractor` để lấy tập tham chiếu
`(mediaId, usageType)` từ markdown trước và sau cập nhật. Với field markdown có
mặt, Application tính:

- `added`: có trong nội dung mới nhưng không có trong nội dung cũ;
- `removed`: có trong nội dung cũ nhưng không có trong nội dung mới.

Sau khi lưu dữ liệu Course/Lesson, service gửi command bất đồng bộ tới Media
Service. Không gọi Media HTTP API và không chờ worker hoàn thành trong response
HTTP. Command chứa owner (`courseId` với `COURSE_DESCRIPTION`, hoặc `lessonId`
với `LESSON_CONTENT`), actor và hai tập chênh lệch.

Media worker tiêu thụ command idempotently. Với `added`, worker chỉ tạo usage
nếu chưa có active usage cùng `(mediaId, ownerService, ownerType, ownerId,
usageType)`. Với `removed`, worker soft-delete active usage đúng owner và loại
media; Media Service chỉ đưa media về draft khi không còn active usage ở bất kỳ
owner nào. Command retry không được tạo hoặc xóa thêm lần hai.

## Kiến trúc

Giữ dependency direction `Api -> Application <- Infrastructure`. API thực hiện
binding của request có khả năng nhận biết field vắng mặt, sau đó chỉ map field
được gửi vào command. Application validate, đọc snapshot hiện tại, áp dụng thay
đổi và tạo diff markdown. Infrastructure cập nhật database Course theo
transaction hiện có. Media Service sở hữu usage và worker xử lý command; không
có service nào truy cập database của service khác.

`COURSE_DESCRIPTION` là owner type mới trong Media Service. `LESSON_CONTENT`
tiếp tục là owner type của Lesson. Không đổi schema Course hoặc Media; command
contract và worker behavior được mở rộng tương thích ngược.

## Kiểm thử

- Unit cho extractor/diff, Course và Lesson handlers: field vắng giữ nguyên,
  null/rỗng xóa markdown, diff add/remove đúng, markdown không đổi không gửi
  command, và các lỗi validation/not-found/conflict.
- Component TestServer cho ba endpoint: binding phân biệt field vắng với
  `null`, response envelope `200`, cùng error envelope an toàn.
- Unit cho Media worker handler/repository boundary: tạo và gỡ usage theo diff,
  retry idempotent và không ảnh hưởng usage của owner khác.
- Cập nhật API docs, business flows, Postman và test catalogs tương ứng.

## Ngoài phạm vi

- Không thay đổi schema/migration.
- Không thêm optimistic concurrency token vì contract hiện tại chưa có version
  hoặc ETag chuẩn.
- Không sửa markdown fields ngoài Course description và Lesson content.
