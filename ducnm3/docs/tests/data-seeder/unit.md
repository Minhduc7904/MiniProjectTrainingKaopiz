# Kiểm thử đơn vị Data Seeder

## Dự án

`backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj`

## Bộ sinh xác định

Mã nguồn: `DeterministicSeedDataTests.cs`.

### `SameSeedAndIndexProduceSameRows`

- Thiết lập: hai bộ sinh có tùy chọn và hạt giống ngẫu nhiên giống nhau.
- Đầu vào: cùng chỉ mục học viên, khóa học, bài học và ghi danh.
- Đạt: toàn bộ bản ghi được sinh và các phép gán khóa học giống nhau.

### `DifferentRandomSeedsProduceDifferentIdsAndAssignments`

- Thiết lập: hai bộ sinh có hạt giống ngẫu nhiên khác nhau.
- Đầu vào: chỉ mục thực thể giống nhau.
- Đạt: ID học viên/khóa học và các phép gán khóa học khác nhau, chứng minh các hạt giống được cô lập.

### `GeneratedRelationshipsStayWithinConfiguredRangesAndRemainUnique`

- Thiết lập: sinh 20 khóa học và phép gán cho 50 học viên.
- Đầu vào: khoảng bài học `1-5`, khoảng ghi danh `1-10`.
- Đạt: mọi số lượng đều nằm trong khoảng; một học viên không bao giờ nhận chỉ mục
  khóa học trùng lặp; mọi chỉ mục khóa học đều tồn tại.

### `CalculatePlanMatchesGeneratedRelationshipCounts`

- Thiết lập: tính kế hoạch và liệt kê độc lập các quan hệ được sinh.
- Đạt: tổng cố định của học viên/khóa học và tổng tính toán của bài học/ghi danh khớp nhau.

### `CalculatePlanStudentsOnlyExcludesCourseOwnedRows`

- Thiết lập: `StudentsOnly = true` với số Student xác định.
- Đạt: kế hoạch chỉ có Student; Course, Lesson, Enrollment và LessonProgress đều bằng `0`.

## Kiểm tra tùy chọn an toàn

Mã nguồn: `SeedOptionsTests.cs`.

- `ValidateAcceptsConfirmedDevelopmentConfiguration`: tùy chọn Development hợp lệ không gây ngoại lệ.
- `ValidateAcceptsConfirmedSeedDatabaseConfiguration`: kết nối đến
  `lms_student_seed_db` và `lms_course_seed_db` trong môi trường Development được chấp nhận.
- `ValidateRejectsEnvironmentOtherThanDevelopment`: Production bị từ chối.
- `ValidateRejectsWriteWithoutConfirmation`: thao tác ghi không có `--confirm` bị từ chối.
- `ValidateAllowsDryRunWithoutConfirmation`: cho phép chạy thử chỉ đọc.
- `ValidateStudentsOnlyWithoutCourseConnectionAcceptsConfiguration`: mode
  `--students-only` chỉ yêu cầu connection string của Student database.
- `ParseOptionsStudentsOnlyDoesNotRequireCourseConnection`: CLI nhận
  `--students-only` và không yêu cầu Course connection string.
- `ValidateRejectsUnexpectedDatabaseName`: kết nối không trỏ đến `lms_course_db`
  hoặc `lms_course_seed_db` bị từ chối.
- `OptimizeCourseSeedSchemaFreshSeedDatabaseRunReturnsTrue`: chỉ fresh run trên
  `lms_course_seed_db` mới bật tối ưu tạm thời schema Course.
- `OptimizeCourseSeedSchemaResumeOrDryRunReturnsFalse`: `--resume` và
  `--dry-run` luôn giữ nguyên constraint/index, kể cả với database seed.
- `OptimizeCourseSeedSchemaDevelopmentDatabaseReturnsFalse`: `lms_course_db`
  production-development không bao giờ bị gỡ constraint/index.
- `ValidateRejectsInvalidCourseAssignmentRange`: khoảng gán bằng không, đảo ngược
  hoặc nằm ngoài giới hạn bị từ chối.

Chạy:

```bash
dotnet test backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj
```
