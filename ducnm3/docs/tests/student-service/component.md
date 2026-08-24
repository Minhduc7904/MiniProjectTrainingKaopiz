# Component test Student Service

## Phạm vi

Dự án:
`backend/Services/Student/StudentService.ComponentTests/StudentService.ComponentTests.csproj`

Mã nguồn:
`Endpoints/GetStudentsEndpointComponentTests.cs`

Dependency: ASP.NET Core `TestServer`; `IStudentListRepository` được thay bằng
stub trong memory. Test không dùng MySQL, Docker hoặc network thật.

Chạy:

```bash
dotnet test backend/Services/Student/StudentService.ComponentTests/StudentService.ComponentTests.csproj
```

## Ca kiểm thử

- `GetStudentsSummaryEndpointComponentTests`: `GET /api/students/summary` với actor ADMIN trả `200`, `totalStudents`, envelope chuẩn và `Cache-Control: no-store`; thiếu actor trả `400 VALIDATION_FAILED`.

- `DefaultQueryReturnsOffsetPaginationEnvelope`: gọi `GET /api/students` không
  query; đạt khi response là `200`, `data` là array, `Cache-Control: no-store`
  và `meta.pagination` có đầy đủ type/page/pageSize/totalItems/totalPages.
- `AllowlistedQueryMapsNormalizedRepositoryQuery`: gửi search, status, sort, direction,
  page và pageSize; đạt khi HTTP trả `200` và repository nhận query đã trim/chuẩn hóa.
- `InvalidQueryReturnsValidationEnvelope`: gửi đồng thời năm parameter sai; đạt
  khi response là `400 VALIDATION_FAILED`, có năm details và repository không
  được gọi.
- `EmptyPageReturnsSuccessWithZeroTotals`: repository trả list rỗng; đạt khi
  endpoint vẫn trả `200`, `data: []`, giữ page được yêu cầu và totals bằng `0`.

## Điều kiện cô lập

- Host gọi production `AddStudentApplication`, shared middleware và
  `MapGetStudents`.
- Request dùng `ApiRoutes.Students`.
- Mỗi test tạo/dispose host và client riêng.
