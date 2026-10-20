# Tạo Quiz cho Lesson

Business flow: [`post-course-lesson-quizzes.md`](../../../business-flows/course-learning/post-course-lesson-quizzes.md).

`POST /course/api/courses/{courseId}/lessons/{lessonId}/quizzes` — Tạo một Quiz kèm câu hỏi và lựa chọn trong một transaction.

Header bắt buộc: `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ. `courseId`, `lessonId`, `quizId` phải là UUID.

## Request

```json
{
  "title": "Kiểm tra cuối bài",
  "descriptionMarkdown": "Làm **cẩn thận**, công thức $x^2$ hiển thị bằng KaTeX.",
  "status": "PUBLISHED",
  "questions": [
    {
      "questionMarkdown": "Thủ đô của Việt Nam?",
      "questionType": "SINGLE_CHOICE",
      "points": 2,
      "options": [
        { "optionMarkdown": "Hà Nội", "isCorrect": true },
        { "optionMarkdown": "Huế", "isCorrect": false }
      ]
    },
    {
      "questionMarkdown": "Chọn các số chẵn",
      "questionType": "MULTIPLE_CHOICE",
      "points": 3.5,
      "options": [
        { "optionMarkdown": "2", "isCorrect": true },
        { "optionMarkdown": "3", "isCorrect": false },
        { "optionMarkdown": "4", "isCorrect": true }
      ]
    }
  ]
}
```

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

## Response

`201 Created`, header `Location: /course/api/courses/{courseId}/lessons/{lessonId}/quizzes/{quizId}`, body envelope chuẩn với `data` là Quiz đầy đủ (`id`, `title`, `descriptionMarkdown`, `descriptionHtml`, `displayOrder`, `status`, `questionCount`, `totalPoints`, `hasAttempts`, `questions[].options[]` kèm `isCorrect`). `displayOrder` tự gán cuối danh sách Quiz của Lesson.

## Lỗi

- `400 VALIDATION_FAILED`: vi phạm một trong các quy tắc trên, kèm `details[].field` như `questions[0].options`.
- `403 FORBIDDEN`: actor không phải ADMIN.
- `404 LESSON_NOT_FOUND`: Lesson không tồn tại trong Course.
