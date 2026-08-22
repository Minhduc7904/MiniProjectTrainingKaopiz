# `GET /course/api/student/enrollments/{courseId}/lessons/{lessonId}` — Nội dung Lesson Student

API contract: [`get-student-lesson-detail.md`](../../../api/course-service/endpoints/get-student-lesson-detail.md).

## Mục tiêu

Cho Student đang học mở một Lesson thuộc Course đã ghi danh, đọc nội dung HTML an toàn và các media attachment.

## Điều kiện trước

- Actor là Student; `courseId` và `lessonId` hợp lệ.
- Enrollment `(course_id, student_id)` tồn tại.

## Luồng chính

1. Player gửi GET không body qua Gateway.
2. Course Service kiểm tra enrollment của actor.
3. Service đọc Lesson thuộc Course và lấy attachment metadata qua Media reader.
4. Service render/sanitize Markdown, trả response envelope `200` với `Cache-Control: no-store`.

## Luồng lỗi

- `400 VALIDATION_FAILED`: UUID route sai hoặc rỗng.
- `403 STUDENT_NOT_ENROLLED`: actor không có enrollment.
- `404 LESSON_NOT_FOUND`: Lesson không thuộc Course hoặc đã bị xóa.

## Dữ liệu và side effects

Chỉ đọc Course database và Media metadata; không cập nhật `lesson_progresses`, không publish message và không tạo job.
