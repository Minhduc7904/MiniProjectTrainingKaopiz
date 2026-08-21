# Kiểm thử thành phần Course Service

## Phạm vi

Dự án: `backend/Services/Course/CourseService.ComponentTests`.
Thành phần phụ thuộc: ASP.NET Core `TestServer` và repository double trong memory;
không dùng MySQL hay mạng.

Chạy:

```bash
dotnet test backend/Services/Course/CourseService.ComponentTests/CourseService.ComponentTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Request | Đạt khi |
| --- | --- | --- |
| `DefaultQueryReturnsOffsetPaginationEnvelope` | `GET /course/api/courses` | Trả `200`, envelope chuẩn, `meta.pagination` offset mặc định và `Cache-Control: no-store`. |
| `InvalidQueryReturnsValidationEnvelopeWithoutCallingRepository` | Query có status, sort, direction, page và pageSize sai. | Trả `400 VALIDATION_FAILED`, năm chi tiết lỗi và không gọi repository. |
| `CreateCourseAlwaysCreatesDraftAndSynchronizesDescriptionMedia` | `POST /course/api/courses` với `status=PUBLISHED` thêm vào JSON và Markdown image Media. | Trả `201`, `Location` tới Course detail, repository nhận `DRAFT` và gửi đúng command đồng bộ `COURSE_DESCRIPTION`. |
| `CreateCourseWithoutActorHeaderReturnsValidationErrorBeforePersisting` | `POST /course/api/courses` không có `X-Actor-Id`. | Trả `400 VALIDATION_FAILED` và không gọi repository để tạo Course. |
| `Delete_ExistingCourse_ReturnsAcceptedAndQueuesUsageCleanup` | `DELETE /course/api/courses/{courseId}` với admin actor và các boundary double. | Trả `202` body rỗng, Course bị xóa và command dọn usage được gửi sau đó. |
| `ExportCoursesEndpointComponentTests` | `GET /course/api/courses/export` với status hợp lệ/sai. | CSV có UTF-8 BOM, attachment/no-store và escaping đúng; status sai trả JSON `400` trước stream. |
