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

## Tập dữ liệu API Course cho Development và seed

Profile `course-api-large` tạo đúng một Lesson và một LessonProgress cho mỗi
Course. Dùng hai lệnh sau khi các bảng đích đang rỗng:

```bash
# Database Development: 100,000 Student, 300,000 Course/Lesson/LessonProgress.
scripts/seed/run-development-seed.sh \
  --env-file .env \
  --confirm \
  --profile course-api-large \
  --students 100000 \
  --courses 300000 \
  --min-lessons 1 \
  --max-lessons 1 \
  --batch-size 2000 \
  --random-seed 20260825

# Database seed: 100,000 Student, 3,000,000 Course/Lesson/LessonProgress.
scripts/seed/run-development-seed.sh \
  --env-file .env.seed \
  --confirm \
  --profile course-api-large \
  --students 100000 \
  --courses 3000000 \
  --min-lessons 1 \
  --max-lessons 1 \
  --batch-size 2000 \
  --random-seed 20260824
```

Hai lệnh còn tạo `1,000,000` Enrollment (10 Course/Student). Nếu cần kiểm tra
schema và kế hoạch trước khi ghi, thay `--confirm` bằng `--dry-run`.

## Chỉ seed Student vào database hiện tại

Dùng `--students-only` để chỉ tạo Student trong `lms_student_db.students`; công cụ
không mở kết nối hoặc ghi vào Course database. Chế độ này có thể giữ lại Student
thủ công đang có trong database hiện tại. Nó chỉ kiểm tra số bản ghi mang email
deterministic của đúng `random-seed`, nên không trộn hai tập seed khác nhau.

```bash
scripts/seed/run-development-seed.sh \
  --confirm \
  --students-only \
  --students 100000 \
  --random-seed 20260813
```

Nếu lần chạy bị ngắt, dùng lại toàn bộ tùy chọn trên kèm `--resume`. Không dùng
`--resume` với `random-seed` khác.

Giới hạn:

- Học viên: `1-100,000`; Khóa học: `1-300,000` (mặc định vẫn là `100,000`).
- Profile `course-api-large` cho phép `3,000,000` Course, đúng `1` Lesson/Course,
  khoảng `1,000,000` Enrollment và `3,000,000` LessonProgress.
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

### Tối ưu schema tạm thời cho database seed

Fresh run ghi vào `lms_course_seed_db` tự động gỡ tạm thời toàn bộ foreign key,
secondary index, unique index và primary key của `courses`, `lessons`,
`enrollments`, `lesson_progresses`. Terminal hiển thị từng thao tác `Drop` và
`Restore`; lỗi khi gỡ một mục được đánh dấu `FAILED — continue` và seed vẫn tiếp
tục. Chỉ constraint/index thực sự gỡ thành công mới được khôi phục sau seed.

Trước khi validation kết quả chạy, tool dựng lại primary key, index/unique index,
rồi foreign key theo thứ tự này. Nếu một mục đã gỡ không thể khôi phục, tool kết
thúc với lỗi để database không bị dùng khi schema còn thiếu.

`lms_course_db`, `--dry-run`, `--resume` và `--students-only` không áp dụng tối
ưu này. `--resume` giữ nguyên schema để bảo toàn idempotency của insert; chỉ dùng
fresh run trên database seed rỗng khi cần tốc độ bulk insert cao.

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

## Seed profile Course API lớn

Profile này tạo dataset quan hệ đủ lớn cho index, pagination, export và API
Course details. Reset trước khi chạy để tránh trộn dataset:

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build

scripts/seed/run-development-seed.sh \
  --confirm \
  --profile course-api-large \
  --random-seed 20260820 \
  --batch-size 2000
```

Kế hoạch mặc định của profile:

```text
students           100,000
courses          3,000,000
lessons          3,000,000
enrollments      1,000,000
lesson_progresses 3,000,000
```

Kiểm tra sau seed:

```bash
docker compose exec -T mysql mysql --batch --skip-column-names \
  --user=root --password="$MYSQL_ROOT_PASSWORD" \
  --execute="
    SELECT 'students', COUNT(*) FROM lms_student_db.students;
    SELECT 'courses', COUNT(*) FROM lms_course_db.courses;
    SELECT 'lessons', COUNT(*) FROM lms_course_db.lessons;
    SELECT 'enrollments', COUNT(*) FROM lms_course_db.enrollments;
    SELECT 'lesson_progresses', COUNT(*) FROM lms_course_db.lesson_progresses;
  "
```

Không tính thời gian reset/seed vào số đo API. Dataset này có thể cần nhiều
thời gian và dung lượng MySQL; nếu bị ngắt, chạy lại đúng lệnh với `--resume`.

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
