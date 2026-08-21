# Xóa Lesson

Business flow: [`delete-course-lesson.md`](../../../business-flows/course-learning/delete-course-lesson.md).

`DELETE /course/api/courses/{courseId}/lessons/{lessonId}` hard-delete một Lesson và trả `202 Accepted` body rỗng. Course Service snapshot media usage thuộc `LESSON_CONTENT` và `LESSON_ATTACHMENT`, xóa Lesson, rồi gửi batch ID sang Media Worker để dọn bất đồng bộ. Request lặp sau khi xóa trả `404 LESSON_NOT_FOUND`.

Yêu cầu `X-Actor-Id`, `courseId` và `lessonId` là UUID hợp lệ. Không dùng distributed transaction.
