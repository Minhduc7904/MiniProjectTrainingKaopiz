# Xem lại một lần làm

API contract: [`GET Xem lại một lần làm`](../../api/course-service/endpoints/get-student-quiz-attempt-detail.md).

Kết quả được chấm lại từ đáp án đã lưu bằng `QuizGrader` thay vì lưu từng câu, nên không cần bảng kết quả riêng; `score`/`maxScore` snapshot lúc nộp nằm ở `quiz_attempts`.
