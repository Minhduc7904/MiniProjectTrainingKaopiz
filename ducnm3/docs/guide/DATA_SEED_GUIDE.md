# Hướng dẫn tạo dữ liệu phát triển

`Lms.DataSeeder` tạo tập dữ liệu lớn trực tiếp trong cơ sở dữ liệu phát triển hiện tại.
Đây là công cụ dòng lệnh chạy thủ công, không phải migration, điểm cuối API hay dịch vụ
chạy nền.

## Tập dữ liệu mặc định

- `100,000` học viên trong `lms_student_db.students`.
- `100,000` khóa học trong `lms_course_db.courses`.
- Mỗi khóa học có ngẫu nhiên `1-5` bài học.
- Mỗi học viên ghi danh ngẫu nhiên vào `1-10` khóa học.
- Không tạo `lesson_progresses`, dữ liệu đa phương tiện, thông báo hoặc dữ liệu lập lịch.

Hạt giống ngẫu nhiên mặc định là `20260813`. ID, số lượng bài học và việc gán khóa
học được sinh xác định từ hạt giống ngẫu nhiên và chỉ mục nên cùng cấu hình luôn
tạo ra cùng một tập dữ liệu. Email được tạo dùng tên miền không gửi thư `example.test`.

## Chạy bằng Docker

Từ thư mục `ducnm3/`:

```bash
scripts/seed/run-development-seed.sh --confirm
```

Tập lệnh sẽ:

1. kiểm tra `.env`, `ASPNETCORE_ENVIRONMENT=Development` và đúng tên cơ sở dữ liệu;
2. khởi động MySQL, Course Service và Student Service để áp dụng migration;
3. đợi các bảng đích sẵn sàng;
4. dựng container `data-seeder` thuộc cấu hình Compose `seed`;
5. tạo dữ liệu, kiểm tra kết quả rồi xóa container chạy một lần.

`data-seeder` không chạy trong `docker compose up` thông thường vì có
`profiles: ["seed"]`.

## Chạy thử và tập dữ liệu nhỏ

Kiểm tra schema, kết nối và số bản ghi dự kiến mà không ghi dữ liệu:

```bash
scripts/seed/run-development-seed.sh --dry-run
```

Tập dữ liệu nhỏ để phát triển hoặc kiểm tra:

```bash
scripts/seed/run-development-seed.sh \
  --confirm \
  --students 1000 \
  --courses 500 \
  --min-lessons 1 \
  --max-lessons 3 \
  --min-courses-per-student 1 \
  --max-courses-per-student 5 \
  --batch-size 500 \
  --random-seed 12345
```

Giới hạn:

- Học viên/Khóa học: `1-100,000`.
- Bài học/Khóa học: `1-5`.
- Khóa học/Học viên: `1-10` và không lớn hơn tổng số khóa học.
- Kích thước lô: `1-2,000`.

## Giao diện tiến trình trên terminal

Trong khi chạy, giao diện hiển thị từng giai đoạn `Students`, `Courses`, `Lessons`,
`Enrollments`, `Validation` với:

- số bản ghi đã xử lý và phần trăm;
- số bản ghi đã chèn/đã có;
- tốc độ bản ghi/giây và thời gian hoàn thành dự kiến;
- tổng số bản ghi và thời gian sau khi hoàn tất.

Mỗi lô dùng một lệnh chèn nhiều bản ghi có tham số và một giao dịch. Công cụ không
giữ toàn bộ tập dữ liệu trong RAM.

## An toàn và tiếp tục

Các điều kiện bắt buộc trước khi ghi:

- môi trường phải chính xác là `Development`;
- chuỗi kết nối phải trỏ tới `lms_student_db` và `lms_course_db`;
- phải có cờ `--confirm`;
- migration `001` và các bảng đích phải tồn tại;
- chỉ một tiến trình tạo dữ liệu được chạy nhờ khóa tư vấn của MySQL;
- mặc định các bảng `students`, `courses`, `lessons`, `enrollments` phải rỗng.

Nếu tiến trình bị hủy hoặc lỗi giữa chừng, chạy lại đúng tùy chọn và hạt giống ngẫu nhiên với
`--resume`:

```bash
scripts/seed/run-development-seed.sh \
  --confirm \
  --resume \
  --random-seed 20260813
```

Thao tác chèn có tính lũy đẳng theo UUID xác định. `--resume` chỉ dành cho chính
tập dữ liệu bị gián đoạn; không dùng nó để trộn dữ liệu thủ công hoặc hạt giống
ngẫu nhiên khác. Bước kiểm tra cuối yêu cầu số bản ghi khớp chính xác kế hoạch.

## Đặt lại toàn bộ dữ liệu được tạo

Dữ liệu được tạo nằm trong cơ sở dữ liệu thật của Course và Student Service, vì
vậy việc đặt lại cơ sở dữ liệu phát triển hiện có sẽ xóa toàn bộ dữ liệu này:

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

Lệnh đặt lại sẽ xóa/tạo lại cả năm cơ sở dữ liệu MySQL và lịch sử migration, sau
đó API áp dụng lại `V001`. MinIO không bị thay đổi. Không cần và không có
migration hoàn tác riêng cho dữ liệu được tạo.

## Chạy công cụ trực tiếp trên máy chủ

Cần .NET SDK 10 và MySQL/schema đã chạy:

```bash
set -a
. ./.env
set +a

export SEED_STUDENT_DB_CONNECTION_STRING="$STUDENT_DB_LOCAL_CONNECTION_STRING"
export SEED_COURSE_DB_CONNECTION_STRING="$COURSE_DB_LOCAL_CONNECTION_STRING"

dotnet run --project backend/Tools/Lms.DataSeeder/Lms.DataSeeder.csproj -- \
  --confirm
```

Xem mọi tùy chọn:

```bash
dotnet run --project backend/Tools/Lms.DataSeeder/Lms.DataSeeder.csproj -- --help
```

Không ghi nhật ký chuỗi kết nối hoặc mật khẩu; bảng cấu hình chỉ hiển thị
máy chủ/cổng/cơ sở dữ liệu.

## Kiểm tra bằng SQL

```sql
SELECT COUNT(*) FROM lms_student_db.students;
SELECT COUNT(*) FROM lms_course_db.courses;
SELECT COUNT(*) FROM lms_course_db.lessons;
SELECT COUNT(*) FROM lms_course_db.enrollments;

SELECT MIN(lesson_count), MAX(lesson_count)
FROM (
    SELECT course_id, COUNT(*) AS lesson_count
    FROM lms_course_db.lessons
    GROUP BY course_id
) AS lesson_distribution;

SELECT MIN(course_count), MAX(course_count)
FROM (
    SELECT student_id, COUNT(*) AS course_count
    FROM lms_course_db.enrollments
    GROUP BY student_id
) AS enrollment_distribution;
```

Với cấu hình mặc định, hai khoảng phải lần lượt là `1-5` và `1-10`.

## Khắc phục sự cố

- `target tables are not empty`: đặt lại cơ sở dữ liệu, hoặc chỉ dùng `--resume`
  khi tiếp tục đúng tập dữ liệu bị gián đoạn.
- `does not contain migration version 001`: khởi động lại Course/Student Service
  để áp dụng migration.
- `Another development data seed process is already running`: đợi tiến trình
  hiện tại kết thúc; khóa tự được MySQL giải phóng nếu kết nối đóng.
- `final row counts do not match`: dữ liệu hiện tại không thuộc đúng tập dữ liệu
  xác định; đặt lại rồi tạo dữ liệu lại.
- Hủy bằng `Ctrl+C`: lô đang chạy sẽ hoàn tác; các lô đã commit được giữ để tiếp tục.
