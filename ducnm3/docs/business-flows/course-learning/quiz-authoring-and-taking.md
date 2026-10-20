# Quiz trắc nghiệm: soạn và làm bài

Tổng quan luồng nghiệp vụ của Quiz trong Course Service. Từng API có flow riêng cùng thư mục
(`post-course-lesson-quizzes.md`, `post-student-quiz-attempts.md`, …).

## Mô hình

Lesson có nhiều Quiz; Quiz có nhiều câu hỏi; câu hỏi có nhiều lựa chọn. Câu hỏi là `SINGLE_CHOICE` hoặc
`MULTIPLE_CHOICE`, mỗi câu có điểm riêng và Quiz có tổng điểm. Nội dung mô tả, câu hỏi và lựa chọn là Markdown
giống Lesson (ảnh Media, bảng, code, công thức KaTeX).

## Admin soạn Quiz

1. Admin mở `Course → Lesson → Quiz` và soạn Quiz lồng nhau; có thể lưu `DRAFT` trước và `PUBLISHED` sau.
2. Course Service validate, ghi cả cây trong một transaction và đồng bộ media usage (`QUIZ_CONTENT`) sang Media Worker.
3. Khi Quiz đã có bất kỳ bài làm nào, cây câu hỏi bị khóa (`409 QUIZ_HAS_ATTEMPTS`) để giữ nhất quán điểm cũ.

## Học viên làm bài

1. Học viên đã ghi danh mở bài học, thấy các Quiz `PUBLISHED` của bài ở thanh bên phải và thẻ “Bài trắc nghiệm”.
2. **Không giới hạn thời gian.** Bắt đầu tạo lần làm `IN_PROGRESS`; mỗi lựa chọn được tự lưu nháp lên server.
3. Thoát, tải lại hoặc quay lại Quiz thì “Tiếp tục làm bài” trả đúng lần đang làm dở kèm đáp án đã lưu.
4. Chỉ khi bấm **Nộp bài** mới chấm điểm: câu đạt khi tập đáp án chọn đúng bằng tập đáp án đúng. Lần làm chuyển `SUBMITTED` và bất biến.
5. Học viên xem kết quả từng câu (đáp án đã chọn so với đáp án đúng), làm lại để tạo lần mới và xem lại mọi lần trước.

## Dữ liệu thay đổi

`quizzes`, `quiz_questions`, `quiz_options`, `quiz_attempts`, `quiz_attempt_answers` trong `lms_course_db`;
`media_usages` (owner `QUIZ_CONTENT`) trong `lms_media_db`. Xóa Quiz, Lesson hoặc Course dọn usage tương ứng.
