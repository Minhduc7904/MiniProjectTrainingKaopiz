# Xóa Quiz

API contract: [`DELETE Xóa Quiz`](../../api/course-service/endpoints/delete-course-lesson-quiz.md).

Course Service snapshot active usage ID của owner `QUIZ_CONTENT`/quizId, xóa Quiz, rồi gửi `DeleteMediaUsagesByIdsV1` cho Media Worker. Xóa Lesson hoặc Course cũng snapshot và dọn usage của mọi Quiz bên trong vì FK cascade không báo Media.
