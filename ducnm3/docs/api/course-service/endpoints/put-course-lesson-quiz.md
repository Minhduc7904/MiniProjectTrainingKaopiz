# Cập nhật Quiz

Business flow: [`put-course-lesson-quiz.md`](../../../business-flows/course-learning/put-course-lesson-quiz.md).

`PUT /course/api/courses/{courseId}/lessons/{lessonId}/quizzes/{quizId}` — Cập nhật tiêu đề, mô tả, trạng thái và (khi chưa có bài làm) toàn bộ cây câu hỏi.

Header bắt buộc: `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ. `courseId`, `lessonId`, `quizId` phải là UUID.

## Request

Cùng schema với [POST](post-course-lesson-quizzes.md); `status` là bắt buộc khi cập nhật.

| Field | Quy tắc |
| --- | --- |
| `title` | Bắt buộc, sau trim dài 2–200 ký tự. |
| `descriptionMarkdown` | Tùy chọn, tối đa 20 000 ký tự; rỗng được lưu là `null`. |
| `status` | `DRAFT` hoặc `PUBLISHED` (mặc định `DRAFT` khi tạo). Xuất bản cần ít nhất 1 câu hỏi. |
| `questions` | Tối đa 100 câu. Khi cập nhật, `null`/bỏ qua nghĩa là giữ nguyên cây câu hỏi; có giá trị nghĩa là thay toàn bộ. |
| `questions[].questionMarkdown` | Bắt buộc, tối đa 20 000 ký tự. Markdown như nội dung Lesson (ảnh Media, bảng, code, công thức KaTeX). |
| `questions[].questionType` | `SINGLE_CHOICE` (đúng 1 đáp án đúng) hoặc `MULTIPLE_CHOICE` (ít nhất 1 đáp án đúng). |
| `questions[].points` | Lớn hơn 0, tối đa 100, tối đa 2 chữ số thập phân. Tổng điểm Quiz = tổng điểm các câu, không lưu thành cột. |
| `questions[].options` | 2–10 lựa chọn; `optionMarkdown` bắt buộc, tối đa 2 000 ký tự. |

Markdown được server render thành HTML đã sanitize (`descriptionHtml`, `questionHtml`, `optionHtml`). Ảnh dùng URL `/media/api/media/{mediaId}/content` được đồng bộ sang Media Service với owner `QUIZ_CONTENT` (owner ID là Quiz); usage được diff khi sửa và dọn khi xóa Quiz/Lesson/Course.

Nếu Quiz **đã có bất kỳ bài làm nào** (kể cả đang làm dở), chỉ được đổi `title`, `descriptionMarkdown`, `status`; gửi `questions` có giá trị sẽ bị từ chối để giữ bài làm và lịch sử điểm nhất quán. Muốn đổi câu hỏi thì tạo Quiz mới hoặc xóa Quiz.

## Response

`200 OK` với Quiz đầy đủ như response tạo.

## Lỗi

- `400 VALIDATION_FAILED`.
- `403 FORBIDDEN`.
- `404 LESSON_NOT_FOUND`, `404 QUIZ_NOT_FOUND`.
- `409 QUIZ_HAS_ATTEMPTS`: gửi `questions` cho Quiz đã có bài làm.
