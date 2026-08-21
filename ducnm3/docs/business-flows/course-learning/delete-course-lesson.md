# Xóa Lesson

API contract: [`DELETE Lesson`](../../api/course-service/endpoints/delete-course-lesson.md).

Course Service lấy active usage ID theo hai scope Markdown/attachment của Lesson, hard-delete bản ghi Lesson, sau đó gửi `DeleteMediaUsagesByIdsV1` đến Media Worker. Worker bỏ qua usage ID đã bị xóa để command idempotent.
