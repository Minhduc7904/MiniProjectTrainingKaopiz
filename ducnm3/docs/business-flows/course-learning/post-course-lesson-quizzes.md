# Tạo Quiz cho Lesson

API contract: [`POST Tạo Quiz cho Lesson`](../../api/course-service/endpoints/post-course-lesson-quizzes.md).

Course Service validate và normalize input bằng `QuizValidation`, kiểm tra Lesson thuộc Course, ghi Quiz → Question → Option trong một transaction, rồi trích URL media từ toàn bộ Markdown (mô tả + câu hỏi + lựa chọn). Nếu có tham chiếu media, gửi `SynchronizeMarkdownMediaUsageV1` (`COURSE`/`QUIZ_CONTENT`/quizId) cho Media Worker. Không dùng distributed transaction; usage được đồng bộ bất đồng bộ.
