# Nộp bài và chấm điểm

API contract: [`POST Nộp bài và chấm điểm`](../../api/course-service/endpoints/post-student-quiz-attempt-submit.md).

Bài làm đã nộp là bất biến. Cập nhật chuyển trạng thái dùng `ExecuteUpdate` có điều kiện `status = IN_PROGRESS` trong transaction; hai request nộp đồng thời chỉ một request thắng, request còn lại nhận `409`. Chấm điểm bằng hàm thuần `QuizGrader`.
