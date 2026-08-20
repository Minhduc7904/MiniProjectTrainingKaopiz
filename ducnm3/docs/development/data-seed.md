# Chiến lược seed dữ liệu phát triển

Các tập dữ liệu phát triển lớn được tạo bằng
`backend/Tools/Lms.DataSeeder`, không phải qua endpoint HTTP hoặc migration SQL.
`scripts/seed/run-development-seed.sh` là điểm vào có chốt bảo vệ dành cho
người dùng, còn profile `seed` của Docker Compose bảo đảm công cụ chỉ chạy khi
được chủ động yêu cầu.

## Quyền sở hữu

- Seeder ghi Student qua kết nối database Student.
- Seeder ghi khóa học, bài học và lượt ghi danh qua kết nối database Course.
- `enrollments.student_id` vẫn là tham chiếu logic xuyên database; không tạo
  foreign key xuyên database.
- Các service nghiệp vụ không tham chiếu project seeder.

Công cụ console là một adapter phát triển nằm ngoài runtime của service. Công
cụ tái sử dụng các hợp đồng database vật lý vì khối lượng công việc này phục vụ dữ liệu
hiệu năng/demo cục bộ; việc chèn hàng trăm nghìn bản ghi qua API sẽ làm sai lệch
cả thời gian chạy lẫn kết quả benchmark.

## Tính xác định và tính idempotent

Bộ sinh tạo UUID và lựa chọn quan hệ từ:

```text
random-seed + loại thực thể + chỉ mục xác định
```

Cùng tùy chọn sẽ tạo ra cùng các bản ghi. Thao tác chèn nhiều bản ghi xử lý dữ
liệu trùng bằng thao tác rỗng (no-op), cho phép tiếp tục lần chạy bị gián đoạn với `--resume`.
Random seed khác đại diện cho tập dữ liệu khác và không được trộn vào các bảng
không rỗng.

## Các giai đoạn ghi dữ liệu

1. Kiểm tra hợp lệ môi trường, tên database chính xác, trạng thái sẵn sàng của
   migration/bảng và số bản ghi hiện tại, sau đó lấy khóa tư vấn (advisory lock).
2. Tính chính xác tổng số bài học và lượt ghi danh dự kiến.
3. Seed Student.
4. Seed khóa học.
5. Seed bài học sau khóa học cha.
6. Seed lượt ghi danh sau học viên và khóa học.
7. Seed tiến độ bài học sau khi đã có Lesson và Student.
8. Kiểm tra tổng số chính xác, phạm vi quan hệ và một tham chiếu logic học viên
   xuyên database.

Mỗi lô được tham số hóa và thực thi trong transaction. Kích thước lô mặc định
là `1,000`; công cụ tạo và giải phóng từng lô một.

## Quy mô mặc định

- `100,000` Student.
- `100,000` khóa học.
- `1-5` bài học cho mỗi khóa học.
- `1-10` lượt ghi danh cho mỗi học viên.
- Không có tiến độ bài học.

## Profile Course API lớn

Profile `course-api-large` phục vụ benchmark index, pagination, export và
Course -> Lessons -> Progress:

- `100,000` Student.
- `3,000,000` Course.
- `3,000,000` Lesson, đúng một Lesson/Course.
- khoảng `1,000,000` Enrollment, 10 Enrollment/Student.
- `3,000,000` LessonProgress, một Progress/`Lesson` và Student được phân bổ
   deterministic theo vòng lặp.

Profile này cố ý không tạo Progress cho mọi tổ hợp Student x Lesson; cách đó sẽ
tạo hàng nghìn tỷ dòng và không phản ánh một dataset benchmark có thể vận hành.

```bash
scripts/seed/run-development-seed.sh \
   --confirm \
   --profile course-api-large \
   --random-seed 20260820 \
   --batch-size 2000
```

Profile yêu cầu đúng một Lesson/Course. Không tính thời gian seed vào benchmark
API và nên kiểm tra dung lượng MySQL trước khi chạy.

Xem các lệnh, hành vi resume/reset và cách khắc phục sự cố vận hành tại
[`../guide/DATA_SEED_GUIDE.md`](../guide/DATA_SEED_GUIDE.md).
