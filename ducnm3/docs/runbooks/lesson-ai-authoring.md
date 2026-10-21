# Runbook — Soạn nội dung Lesson bằng AI

Luồng tổng thể: [`lesson-ai-authoring.md`](../business-flows/course-learning/lesson-ai-authoring.md). Cấu hình AI Service: [`ai-service.md`](ai-service.md).

## Chuẩn bị (một lần)

1. Chạy `ai-service`, `course-service`, `media-service`, `api-gateway`. `course-service` cần biến `ServiceEndpoints__ai-service: http://ai-service:8080/` (đã có trong `docker-compose.yml`) và `ServiceEndpoints__media-service`.
2. Trong trang **AI → Providers** thêm provider với API key thật, **AI → Models** thêm model, rồi **AI → Gán model** gán cho ba khóa `document-extraction`, `lesson-synthesis`, `quiz-authoring`.
3. Dùng **Thử kết nối** (`AI → Providers`) để chắc chắn key hợp lệ. Không dùng tài liệu nội bộ/bí mật của công ty để thử: nội dung tài liệu được gửi tới provider.

## Dùng trên giao diện

1. Mở khóa học → chọn Lesson → tab **Tài liệu** → tải tài liệu lên (PDF, DOCX, TXT, MD, ảnh). Upload báo thành công ngay; học viên vẫn xem được tài liệu gốc.
2. Panel **Soạn nội dung bằng AI** tự đồng bộ và hiện trạng thái từng tài liệu: Đang chờ → Đang trích xuất → Đã trích xuất (hoặc Lỗi / Không hỗ trợ).
3. Tick các tài liệu muốn dùng (mặc định không tick gì), rồi bấm **Tự động tổng hợp bài học** (ghi vào nội dung Lesson, hỏi xác nhận nếu Lesson đã có nội dung) hoặc **Tự động tạo quiz** (chọn số câu 1–30; tạo Quiz `DRAFT`).
4. Kiểm tra kết quả: nội dung Lesson hoặc Quiz `DRAFT` (nút **Mở quiz nháp**), sửa nếu cần rồi mới xuất bản Quiz.

## Xử lý sự cố

| Triệu chứng | Nguyên nhân thường gặp | Cách xử lý |
| --- | --- | --- |
| Tài liệu `FAILED` `AI_TASK_NOT_CONFIGURED` | Chưa gán model cho `document-extraction`. | Gán model rồi bấm **Thử lại tài liệu lỗi**. |
| Tài liệu `FAILED` `AI_PROVIDER_REJECTED_REQUEST` | Model/provider không nhận PDF hoặc ảnh. | Đổi model cho `document-extraction`, hoặc tải bản DOCX/ảnh. |
| Tài liệu `FAILED` `MEDIA_TOO_LARGE` | Tệp lớn hơn 15 MB. | Tách nhỏ tệp. |
| Tài liệu `FAILED` `EMPTY_DOCUMENT` | Không đọc được chữ (ảnh trống, PDF scan mờ). | Dùng bản rõ hơn. |
| Tài liệu `FAILED` `WORKER_INTERRUPTED` | Course Service khởi động lại giữa chừng. | Bấm **Thử lại tài liệu lỗi**. |
| Job `FAILED` `DOCUMENTS_TOO_LARGE` | Văn bản các tài liệu chọn vượt 150.000 ký tự. | Chọn ít tài liệu hơn. |
| Job `FAILED` `AI_NO_RESULT` / `AI_TOO_MANY_STEPS` | Model không gọi tool hoặc tạo dữ liệu sai nhiều lần. | Dùng model mạnh hơn, hỗ trợ tool tốt; tạo job lại. |
| Job `FAILED` `WORKER_INTERRUPTED` | Course Service dừng khi job đang chạy. Job không tự chạy lại để tránh tạo trùng Quiz. | Kiểm tra Lesson/Quiz đã có chưa rồi tạo job mới. |
| Nội dung Lesson bị AI ghi đè ngoài ý muốn | Đã xác nhận ghi đè. | Lấy bản cũ ở `GET …/ai-jobs/{jobId}` (`previousContentMarkdown`) và dán lại qua sửa Lesson. |

## Truy vấn hữu ích

```sql
-- lms_course_db: tài liệu đang kẹt hoặc lỗi
SELECT lesson_id, file_name, status, error_code, attempts, locked_until
FROM lesson_attachment_texts WHERE status IN ('EXTRACTING', 'FAILED') ORDER BY updated_at DESC;

-- lms_course_db: job gần đây
SELECT id, lesson_id, job_type, status, step_count, error_code, created_at, finished_at
FROM lesson_ai_jobs ORDER BY created_at DESC LIMIT 20;
```

Worker chạy trong tiến trình `course-service` (2 vòng lặp song song, nghỉ 2 giây khi rảnh); chạy nhiều instance an toàn vì việc được nhận bằng `UPDATE` có điều kiện kèm lease 10 phút.
