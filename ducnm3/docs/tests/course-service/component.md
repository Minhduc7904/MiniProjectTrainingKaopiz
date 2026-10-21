# Kiểm thử thành phần Course Service

## Phạm vi

Dự án: `backend/Services/Course/CourseService.ComponentTests`.
Thành phần phụ thuộc: ASP.NET Core `TestServer` và repository double trong memory;
không dùng MySQL hay mạng.

Không có dự án Course integration test/Testcontainers trong repository hiện tại.
Thay đổi này không đổi schema/index; query persistence được build và use case/HTTP
contract được bao phủ bởi unit + component test với boundary có thể cô lập.

Chạy:

```bash
dotnet test backend/Services/Course/CourseService.ComponentTests/CourseService.ComponentTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Request | Đạt khi |
| --- | --- | --- |
| `GetCoursesSummaryEndpointComponentTests` | `GET /api/courses/summary` với actor ADMIN | Trả `200`, tổng Course/Lesson, envelope chuẩn và `Cache-Control: no-store`. |

| Kiểm thử | Request | Đạt khi |
| --- | --- | --- |
| `DefaultQueryReturnsOffsetPaginationEnvelope` | `GET /course/api/courses` | Trả `200`, envelope chuẩn, `meta.pagination` offset mặc định và `Cache-Control: no-store`. |
| `SearchQueryNormalizesAndPassesSearchFilterToRepository` | `GET /course/api/courses?search=%20Backend%20` | Trả `200` và repository nhận `Search=Backend` đã trim. |
| `InvalidQueryReturnsValidationEnvelopeWithoutCallingRepository` | Query có status, sort, direction, page và pageSize sai. | Trả `400 VALIDATION_FAILED`, năm chi tiết lỗi và không gọi repository. |
| `CreateCourseAlwaysCreatesDraftAndSynchronizesDescriptionMedia` | `POST /course/api/courses` với `status=PUBLISHED` thêm vào JSON và Markdown image Media. | Trả `201`, `Location` tới Course detail, repository nhận `DRAFT` và gửi đúng command đồng bộ `COURSE_DESCRIPTION`. |
| `CreateCourseWithoutActorHeaderReturnsValidationErrorBeforePersisting` | `POST /course/api/courses` không có `X-Actor-Id`. | Trả `400 VALIDATION_FAILED` và không gọi repository để tạo Course. |
| `Delete_ExistingCourse_ReturnsAcceptedAndQueuesUsageCleanup` | `DELETE /course/api/courses/{courseId}` với admin actor và các boundary double. | Trả `202` body rỗng, Course bị xóa và command dọn usage được gửi sau đó. |
| `ExportCoursesEndpointComponentTests` | `GET /course/api/courses/export` với status hợp lệ/sai. | CSV có UTF-8 BOM, attachment/no-store và escaping đúng; status sai trả JSON `400` trước stream. |
| `StudentLearningEndpointsComponentTests` | GET list enrollment/catalog (bao gồm `search` tên Course), Student lesson detail và GET progress với actor không hợp lệ. | List trả `200`, catalog trim `search`, offset envelope, thumbnail, `no-store`; Student lesson detail trả envelope có attachment/content HTML; Admin bị `403 FORBIDDEN`. |

## Quiz (`QuizEndpointsComponentTests`)

Dùng `TestServer`, kho Quiz trong bộ nhớ dùng chung cho cổng admin và học viên, và `ICommandSender` ghi lại.

| Kiểm thử | Request | Đạt khi |
| --- | --- | --- |
| `Create_ValidQuiz_ReturnsCreatedWithRenderedHtmlTotalPointsAndLocation` | `POST …/quizzes` actor ADMIN với 2 câu (chọn 1, chọn nhiều). | `201`, có `Location`, `totalPoints = 5`, `descriptionHtml` có `<strong>`, đáp án đúng được trả cho Admin. |
| `Create_InvalidQuiz_ReturnsValidationFailedWithFieldDetails` | Tiêu đề rỗng, xuất bản không có câu. | `400 VALIDATION_FAILED`, không ghi Quiz. |
| `Create_AsStudent_ReturnsForbidden` / `Create_UnknownLesson_ReturnsNotFound` | Actor STUDENT; Lesson lạ. | `403`; `404 LESSON_NOT_FOUND`. |
| `List_…`, `Detail_UnknownQuiz_ReturnsNotFound` | `GET` danh sách và chi tiết. | Tóm tắt có `questionCount`/`totalPoints`; Quiz lạ trả `404 QUIZ_NOT_FOUND`. |
| `Update_MetadataOnly_KeepsQuestions` / `Update_ReplacingQuestionsAfterAttempt_ReturnsConflict` | `PUT` không có `questions`; `PUT` thay câu hỏi sau khi có bài làm. | `200` giữ nguyên cây; `409 QUIZ_HAS_ATTEMPTS`. |
| `Delete_ExistingQuiz_ReturnsAcceptedAndRemovesIt` | `DELETE`. | `202`, Quiz bị xóa. |
| `StudentDetail_NeverExposesCorrectAnswersOrMarkdownSource` | `GET` đề thi bằng actor STUDENT. | `200`, `no-store`, response không chứa `isCorrect` hay `*Markdown`. |
| `StudentDetail_DraftQuiz_ReturnsNotFound` / `Student_NotEnrolled_ReturnsForbidden` | Quiz `DRAFT`; học viên chưa ghi danh. | `404`; `403 STUDENT_NOT_ENROLLED`. |
| `Student_FullFlow_StartSaveResumeSubmitRetake` | Start → lưu nháp → Start lại → nộp → Start → danh sách. | `201`, `200` cùng attempt với đáp án đã lưu, điểm `5/5` đúng luật, lần mới `201` với id khác, lịch sử có 2 lần. |
| `Submit_Twice_ReturnsConflict`, `SaveAnswers_ForeignOption_ReturnsValidationFailed` | Nộp hai lần; lưu lựa chọn không thuộc câu. | `409 QUIZ_ATTEMPT_ALREADY_SUBMITTED`; `400`. |
| `AttemptDetail_OfAnotherStudent_ReturnsNotFound` | Học viên khác đã ghi danh đọc attempt. | `404 QUIZ_ATTEMPT_NOT_FOUND`. |

`CreateCourseEndpointComponentTests` và `DeleteCourseEndpointComponentTests` gửi thêm `X-Actor-Type: ADMIN`; `DeleteCourseEndpointComponentTests` đăng ký thêm kho Quiz.

## Soạn nội dung Lesson bằng AI

`Endpoints/LessonAiEndpointsComponentTests.cs` dùng `TestServer`, kho trạng thái trong bộ nhớ, Media reader giả và model giả; test gọi `LessonAiWorkRunner` thật để chạy worker đến khi hết việc.

| Kiểm thử | Mục đích / điều kiện đạt |
| --- | --- |
| `SyncQueuesAttachmentsAndTheWorkerExtractsThemIntoTheDocumentList` | `POST …/ai-documents/sync` trả `PENDING`; sau khi worker chạy, `GET …/ai-documents` trả `EXTRACTED` kèm `textLength`, video là `UNSUPPORTED`, `no-store`, không lộ văn bản. |
| `SyncWithRetryFailedRequeuesFailedDocuments` | `?retryFailed=true` đưa tài liệu `FAILED` về `PENDING`. |
| `CreateLessonJobReturnsAcceptedAndTheWorkerWritesTheLessonContent` | `202` kèm `Location`; worker gọi task `lesson-synthesis` và ghi Markdown vào Lesson; job `COMPLETED`. |
| `CreateQuizJobProducesADraftQuizLinkedFromTheJob` | Job `GENERATE_QUIZ` tạo Quiz `DRAFT`, `resultQuizId` trỏ đúng Quiz. |
| `FailedJobExposesTheErrorCodeWithoutLeakingInternals` | Lỗi AI Service được lưu thành `errorCode` của job `FAILED`; Lesson không bị ghi. |
| `CreateJobReturnsConflictWhenOverwriteIsNotConfirmed` | `409 LESSON_CONTENT_EXISTS`, không tạo job. |
| `CreateJobReturnsConflictForDocumentsThatAreNotExtracted` | `409 LESSON_AI_DOCUMENT_NOT_READY`. |
| `CreateJobValidatesTheRequestBody` | `400 VALIDATION_FAILED` với `details` cho `jobType` và `mediaIds`. |
| `SecondActiveJobOfTheSameTypeIsRejected` | `409 LESSON_AI_JOB_ACTIVE`. |
| `UnknownJobAndUnknownLessonReturnNotFound` | `404 LESSON_AI_JOB_NOT_FOUND` / `404 LESSON_NOT_FOUND`. |
| `InvalidRouteIdsReturnValidationFailed` | UUID sai trên route trả `400 VALIDATION_FAILED`. |
| `EveryEndpointRequiresAnAdminActor` | Cả 5 endpoint trả `403` cho Student và `400` khi thiếu header actor; không tạo job. |

Frontend: `frontend/lms-web/src/api/lessonAiApi.test.js`, `src/pages/courses/components/LessonAiPanel.test.jsx` (checkbox mặc định không chọn, chỉ tài liệu `EXTRACTED` chọn được, payload job, hộp xác nhận ghi đè, poll im lặng, báo tải lại Lesson khi job tổng hợp xong, hiển thị lỗi) và `LessonDetailTabs.test.jsx` (panel chỉ mount ở tab Tài liệu).
