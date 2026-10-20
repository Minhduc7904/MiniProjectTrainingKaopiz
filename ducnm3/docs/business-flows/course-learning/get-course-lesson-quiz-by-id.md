# Lấy chi tiết Quiz (Admin)

API contract: [`GET Lấy chi tiết Quiz (Admin)`](../../api/course-service/endpoints/get-course-lesson-quiz-by-id.md).

Course Service kiểm tra Lesson thuộc Course, đọc Quiz theo `(lessonId, quizId)` cùng câu hỏi, lựa chọn và cờ có bài làm, rồi render Markdown sang HTML đã sanitize.
