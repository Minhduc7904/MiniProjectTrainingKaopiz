# Lưu nháp đáp án

API contract: [`PUT Lưu nháp đáp án`](../../api/course-service/endpoints/put-student-quiz-attempt-answers.md).

Frontend debounce ~600 ms sau mỗi lần chọn, mỗi lúc chỉ một request lưu, thử lại với backoff khi lỗi mạng và flush khi đổi tab/đóng trang. Server cập nhật có điều kiện `status = IN_PROGRESS` trong transaction nên không ghi đè bài đã nộp.
