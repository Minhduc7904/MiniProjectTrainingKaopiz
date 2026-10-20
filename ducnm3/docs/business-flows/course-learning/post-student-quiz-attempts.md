# Bắt đầu hoặc tiếp tục làm Quiz

API contract: [`POST Bắt đầu hoặc tiếp tục làm Quiz`](../../api/course-service/endpoints/post-student-quiz-attempts.md).

Mỗi `(quiz, student)` có tối đa một lần `IN_PROGRESS`, bảo đảm ở DB bằng cột sinh `open_marker` + unique `(quiz_id, open_marker)`. Hai request đồng thời cùng tạo lần mới: request thua gặp lỗi unique, đọc lại và trả lần của request thắng. Nhờ vậy thoát/tải lại trang hoặc quay lại Quiz luôn lấy lại bài đang làm.
