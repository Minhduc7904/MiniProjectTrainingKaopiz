# Kiểm thử đơn vị Course Service

## Phạm vi

Dự án: `backend/Services/Course/CourseService.UnitTests`
Mã nguồn: `Features/Courses/GetList/GetCoursesHandlerTests.cs` và health probe.
Thành phần phụ thuộc: không có MySQL hoặc mạng.

Chạy:

```bash
dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `CourseDatabaseHealthProbe` với chuỗi kết nối trỏ tới cổng không hợp lệ; hủy `CancellationToken` trước khi gọi `CheckAsync`. | `CheckAsync` ném `OperationCanceledException`, không chuyển thao tác hủy thành trạng thái cơ sở dữ liệu không khỏe mạnh. |
| `HandleAsync_ValidOffsetQuery_UsesPagedRepositoryOnly` | Tạo query `status=published`, `search= backend ` với repository double. | Query được normalize thành `PUBLISHED` và `backend`; use case chỉ gọi `GetPagedAsync`, không gọi `GetAllAsync`. |
| `Create_WhitespaceSearch_NormalizesToNoFilter` | Tạo query có `search` chỉ gồm khoảng trắng. | `Search` là `null`, không áp dụng filter tên. |
| Export CSV | Test `CsvRowWriter`, export query/chunk và handler với repository double. | BOM/header đúng, escaping RFC 4180, status được validate và export chỉ gọi chunk reader với cancellation token. |
| `HandleAsyncCreatesDraftAndSynchronizesMediaEmbeddedInDescription` | Gọi `CreateCourseHandler` với tên có khoảng trắng và Markdown image Media public. | Handler trim tên, luôn gọi repository với `DRAFT` và gửi đúng command `COURSE_DESCRIPTION` đến Media Service. |
| `HandleAsync_ExistingCourse_DeletesAggregateAndQueuesEveryUsageId` | Repository double trả một Lesson và Media reader trả usage của Course/Lesson. | Handler snapshot đủ năm owner scope, hard-delete Course rồi gửi một `DeleteMediaUsagesByIdsV1` chứa toàn bộ usage ID. |
| `Render_RelativeMediaContentPath_PrefixesConfiguredGatewayPublicBaseUrl` | Renderer nhận Markdown image với URL Media tương đối và biến môi trường Gateway cố định. | HTML trả về có `img src` tuyệt đối, bắt đầu bằng `Gateway__PublicBaseUrl`. |
| `RenderSupportedMathDelimiterEmitsSafeMathElement` | Renderer nhận lần lượt `$...$`, `$$...$$` và `\\[...\\]`. | HTML chỉ chứa placeholder `course-math` do server tạo, với display mode đúng; không tự diễn giải TeX thành HTML. |
| `RenderMathContainsHtmlLikeTextEncodesItInsideMathData` | Renderer nhận TeX chứa chuỗi giống thẻ HTML. | Chuỗi được HTML-encode trong `data-math-tex`; không có thẻ script trong output. |
| Course details (cần bổ sung) | Repository có path batch và path N+1 riêng. | Handler chỉ gọi path batch không N+1; component test xác nhận `400` với UUID sai và `404` khi không có Course. |
| `StudentLearningHandlersTests` | Query page/pageSize sai, catalog Course available, Student chưa ghi danh và Student có progress. | Pagination sai trả `400`; catalog hỗ trợ `search` đã trim và chuyển nguyên result repository; chưa ghi danh trả `403 STUDENT_NOT_ENROLLED`; progress chỉ chứa dữ liệu Student hiện tại. |
| `GetStudentLessonDetailHandlerTests` | Student có/không có enrollment và repository Lesson double. | Enrollment được kiểm tra trước khi đọc Lesson; không có enrollment trả `403 STUDENT_NOT_ENROLLED`; Lesson hợp lệ được trả nguyên projection. |
| `QuizGraderTests` | Quiz một câu chọn 1 (1,5 điểm) và một câu chọn nhiều (2,25 điểm). | Đúng toàn bộ nhận đủ điểm; chọn thiếu hoặc thừa đáp án ở câu chọn nhiều nhận 0; bỏ trống nhận 0 nhưng `MaxScore` giữ nguyên; lựa chọn trùng chỉ tính một lần; trả đúng `CorrectOptionIds` để xem lại. |
| `QuizValidationTests` | Input Quiz thiếu/sai tiêu đề, trạng thái, điểm (0, âm, >100, 3 chữ số thập phân), nội dung rỗng, số lựa chọn, số đáp án đúng theo loại câu. | Mỗi vi phạm ném `VALIDATION_FAILED` với `details[].field` đúng (`questions[0].points`, …); xuất bản không có câu hỏi bị chặn nhưng cho phép khi giữ cây câu hỏi hiện có; `CombineMarkdown` gộp mô tả, câu hỏi và lựa chọn. |
| `QuizAdminHandlersTests` | Create/Update/Delete với kho Quiz trong bộ nhớ và `ICommandSender` ghi lại. | Tạo lưu đủ cây và tổng điểm; chỉ gửi `SynchronizeMarkdownMediaUsageV1` (`QUIZ_CONTENT`) khi Markdown có ảnh; sửa gửi đúng diff thêm/bớt; Quiz đã có bài làm trả `409 QUIZ_HAS_ATTEMPTS` khi thay cây câu hỏi nhưng vẫn cho đổi tiêu đề/trạng thái; xóa gửi `DeleteMediaUsagesByIdsV1` theo `ForQuiz`; Lesson/Quiz không tồn tại trả `404`. |
| `StudentQuizHandlersTests` | Start/Save/Submit/History với học viên đã/chưa ghi danh và Quiz `PUBLISHED`/`DRAFT`. | Start lần đầu tạo mới, lần sau resume đúng bản nháp, sau khi nộp tạo lần mới và giữ lần cũ; lưu nháp thay thế hoàn toàn bản cũ, chấp nhận danh sách rỗng, từ chối lựa chọn lạ/câu trùng/2 lựa chọn ở câu chọn 1; nộp dùng bản nháp hoặc đáp án cuối, chọn nhiều “đúng toàn bộ mới có điểm”, nộp hai lần và lưu sau khi nộp trả `409`; lần làm của học viên khác trả `404`; chưa ghi danh trả `403`; Quiz nháp trả `404`. |
| `DeleteCourseHandlerTests` (mở rộng) | Course có Quiz. | Snapshot thêm owner scope `QUIZ_CONTENT` của từng Quiz và gửi chung một `DeleteMediaUsagesByIdsV1`. |
| `ApiRoutesTests.QuizPathsUseServiceAndGatewayShapes` | Dựng đường dẫn Quiz và attempt. | Service path và gateway path đúng mẫu `/course/api/...`. |
| `LessonAiHandlersTests` | Kho trạng thái AI (`InMemoryLessonAiStore`), `FakeCourseMediaReader` và Lesson trong bộ nhớ. | Sync xếp hàng tệp hỗ trợ, đánh dấu `UNSUPPORTED` cho video/audio, idempotent không reset `EXTRACTED`, chỉ thử lại `FAILED` khi `retryFailed`; tạo job lưu tập tài liệu (gộp ID trùng), mặc định 10 câu, validate loại job/số tài liệu/số câu, từ chối tài liệu chưa `EXTRACTED` hoặc không thuộc Lesson (`LESSON_AI_DOCUMENT_NOT_READY`), đòi `overwriteContent` khi Lesson đã có nội dung (`LESSON_CONTENT_EXISTS`), chặn job cùng loại đang hoạt động (`LESSON_AI_JOB_ACTIVE`); đọc danh sách job và chi tiết kèm nội dung cũ, `LESSON_AI_JOB_NOT_FOUND`. |
| `DocumentExtractionTests` | Media reader giả, model giả theo kịch bản, kho trạng thái trong bộ nhớ. | Phân loại MIME/đuôi tệp; DOCX giữ heading/danh sách/bảng và từ chối tệp không phải DOCX; TXT/MD và DOCX không gọi model; PDF gửi thành phần `file`, ảnh gửi `image` với task `document-extraction` và actor là admin đã yêu cầu; đầu ra bị cắt (`length`) kèm ghi chú; đầu ra rỗng, DOCX hỏng, lỗi Media và lỗi AI Service đều chuyển tài liệu sang `FAILED` đúng mã. |
| `LessonAiJobProcessingTests` | Model giả theo kịch bản, Lesson có thể ghi, kho Quiz trong bộ nhớ. | Vòng lặp agent dừng ngay khi tool ghi thành công, trả lỗi tool lại cho model để tự sửa, thất bại khi model chỉ trả text hoặc vượt 6 lượt; `save_lesson_content` ghi đúng Lesson, đồng bộ media usage, từ chối ghi đè khi chưa duyệt, sao lưu nội dung cũ khi được duyệt, bỏ qua `lessonId` do model gửi, từ chối tool lạ và gọi lần hai; `create_quiz` tạo Quiz luôn `DRAFT` (kể cả khi model đòi `PUBLISHED`), từ chối sai số câu, trả chi tiết validation, chỉ tạo một Quiz; job processor chỉ dùng tài liệu được chọn, bọc trong `<document>`, ghi `resultQuizId`, lưu mã lỗi AI Service, từ chối tài liệu mất văn bản hoặc vượt giới hạn mà không gọi model; work runner ưu tiên trích xuất trước job và ghi `INTERNAL_ERROR` thay vì làm sập worker. |
| `ApiRoutesTests.LessonAiPathsUseServiceAndGatewayShapes` | Dựng đường dẫn tài liệu và job AI. | Service path và gateway path đúng mẫu `/course/api/...`. |

Ca này bảo đảm thao tác tắt/hủy yêu cầu được tôn trọng. Nó không kiểm tra tính
khả dụng của MySQL thật. Ca list bảo vệ việc endpoint phân trang không vô tình
materialize toàn bộ Course.
