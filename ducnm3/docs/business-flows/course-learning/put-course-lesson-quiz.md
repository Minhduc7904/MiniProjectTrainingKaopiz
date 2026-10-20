# Cập nhật Quiz

API contract: [`PUT Cập nhật Quiz`](../../api/course-service/endpoints/put-course-lesson-quiz.md).

Course Service validate với số câu hiện có (để kiểm tra điều kiện xuất bản khi không gửi `questions`), chặn thay cây câu hỏi nếu `HasAttemptsAsync`, ghi trong transaction, rồi tính diff media giữa Markdown cũ và mới và gửi `SynchronizeMarkdownMediaUsageV1` chỉ khi có thêm/bớt.
