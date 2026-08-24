# Kiểm thử tích hợp Data Seeder

## Dự án

`backend/Tools/Lms.DataSeeder.IntegrationTests/Lms.DataSeeder.IntegrationTests.csproj`

Cần có Docker Engine. Kiểm thử khởi động hai Testcontainer MySQL `8.4` cô lập,
một cho `lms_student_db` và một cho `lms_course_seed_db`. Kiểm thử áp dụng migration
SQL `V001` thực của Student/Course qua `SqlMigrationRunner`. Không có cơ sở dữ
liệu của lập trình viên hoặc volume Docker Compose nào bị đọc hay thay đổi.

## `RunAsyncSeedsRelationshipsAndResumeIsIdempotent`

Thiết lập:

- Cơ sở dữ liệu Student chỉ có bảng `students` đã được migration.
- Cơ sở dữ liệu Course chỉ có các bảng `courses`, `lessons`, `enrollments` và
  `lesson_progresses` đã được migration.
- Tập dữ liệu có 30 học viên, 20 khóa học, `1-5` bài học/khóa học và `1-10`
  khóa học/học viên.

Các xác nhận:

1. Lần chạy đầu chèn chính xác 30 học viên và 20 khóa học.
2. Số bản ghi bài học và ghi danh bằng kế hoạch xác định.
3. Không tồn tại nhóm `(course_id, student_id)` trùng lặp.
4. Chạy cùng tập dữ liệu với `Resume = true` không chèn bản ghi mới và báo cáo
   mọi bản ghi theo kế hoạch đã tồn tại.
5. Số bản ghi không đổi sau khi tiếp tục.
6. Việc chạy lại ở chế độ mới bị từ chối vì các bảng đích không rỗng.
7. Fresh run trên `lms_course_seed_db` khôi phục đủ 4 primary key, 3 unique
   constraint, 3 foreign key và 6 secondary index sau khi seed hoàn tất.

Bước kiểm tra cuối của trình chạy, được kiểm thử thực thi, cũng xác minh số lượng
chính xác, khoảng bài học/khóa học, khoảng khóa học/học viên và một tham chiếu học
viên logic xuyên hai cơ sở dữ liệu.

Điều kiện đạt: kiểm thử NUnit hoàn tất mà không có lỗi xác nhận, migration, ràng
buộc MySQL hoặc kiểm tra dữ liệu, đồng thời cả hai container đều được giải phóng.

## `RunAsyncStudentsOnlyPreservesCourseDatabaseAndManualStudents`

Kiểm thử thêm một Student thủ công vào Testcontainer Student rồi chạy mode
`StudentsOnly = true`. Đạt khi tool tạo đúng tập Student deterministic, giữ bản
ghi thủ công và không làm thay đổi số Course trong Testcontainer Course.

Chạy:

```bash
dotnet test backend/Tools/Lms.DataSeeder.IntegrationTests/Lms.DataSeeder.IntegrationTests.csproj
```
