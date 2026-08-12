# Enrollment and Learning Progress

## Mục đích

Student ghi danh vào Course, xem Lesson, và cập nhật tiến độ học.

## Actor

Student; Course Service; Student Service.

## Điều kiện đầu vào

- Student đã được xác thực và có trạng thái `ACTIVE`.
- Course tồn tại và có trạng thái `PUBLISHED`.

## Luồng chính

1. Student chọn một Course để ghi danh.
2. Course Service xác nhận Student qua Student Service hoặc JWT claim.
3. Course Service kiểm tra Course có thể ghi danh và chưa có enrollment active.
4. Course Service tạo `enrollments` với `course_id`, `student_id`, và `enrolled_at`.
5. Student mở Lesson thuộc Course đã ghi danh.
6. Student gửi tiến độ hoàn thành Lesson.
7. Course Service upsert `lesson_progresses` và đặt `completed_at` khi `progress_percent` đạt 100.

## Trường hợp lỗi

- `401`: chưa xác thực.
- `403`: Student không có quyền xem Lesson không thuộc enrollment.
- `404`: Student, Course, hoặc Lesson không tồn tại.
- `409`: Student đã ghi danh Course.

## Dữ liệu thay đổi

- Course Service database: `enrollments`, `lesson_progresses`.
- Student Service database không bị ghi trong luồng này; chỉ được đọc để xác thực/lookup.
