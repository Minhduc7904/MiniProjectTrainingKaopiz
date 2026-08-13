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

## Kiểm tra tùy chọn an toàn

Mã nguồn: `SeedOptionsTests.cs`.

- `ValidateAcceptsConfirmedDevelopmentConfiguration`: tùy chọn Development hợp lệ không gây ngoại lệ.
- `ValidateRejectsEnvironmentOtherThanDevelopment`: Production bị từ chối.
- `ValidateRejectsWriteWithoutConfirmation`: thao tác ghi không có `--confirm` bị từ chối.
- `ValidateAllowsDryRunWithoutConfirmation`: cho phép chạy thử chỉ đọc.
- `ValidateRejectsUnexpectedDatabaseName`: kết nối không trỏ đến `lms_course_db` bị từ chối.
- `ValidateRejectsInvalidCourseAssignmentRange`: khoảng gán bằng không, đảo ngược
  hoặc nằm ngoài giới hạn bị từ chối.

Chạy:

```bash
dotnet test backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj
```
