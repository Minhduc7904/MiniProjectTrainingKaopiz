# Development Data Seed Guide

`Lms.DataSeeder` tạo dataset lớn trực tiếp trong database development hiện tại.
Đây là console tool chạy thủ công, không phải migration, API endpoint hay service
chạy nền.

## Dataset mặc định

- `100,000` Students trong `lms_student_db.students`.
- `100,000` Courses trong `lms_course_db.courses`.
- Mỗi Course có ngẫu nhiên `1-5` Lessons.
- Mỗi Student ghi danh ngẫu nhiên vào `1-10` Courses.
- Không tạo `lesson_progresses`, media, notification hoặc scheduler data.

Random seed mặc định là `20260813`. ID, lesson count và course assignment được
sinh xác định từ random seed và index nên cùng cấu hình luôn tạo cùng dataset.
Email seed dùng domain không gửi mail `example.test`.

## Chạy bằng Docker

Từ thư mục `ducnm3/`:

```bash
scripts/seed/run-development-seed.sh --confirm
```

Script sẽ:

1. kiểm tra `.env`, `ASPNETCORE_ENVIRONMENT=Development` và đúng tên database;
2. khởi động MySQL, Course Service và Student Service để apply migration;
3. đợi các bảng đích sẵn sàng;
4. build container `data-seeder` thuộc Compose profile `seed`;
5. chạy seed, validate kết quả rồi xóa one-shot container.

`data-seeder` không chạy trong `docker compose up` thông thường vì có
`profiles: ["seed"]`.

## Dry run và dataset nhỏ

Kiểm tra schema, connection và số row dự kiến mà không ghi dữ liệu:

```bash
scripts/seed/run-development-seed.sh --dry-run
```

Dataset nhỏ để phát triển hoặc kiểm tra:

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

- Students/Courses: `1-100,000`.
- Lessons/Course: `1-5`.
- Courses/Student: `1-10` và không lớn hơn tổng Courses.
- Batch size: `1-2,000`.

## Terminal progress UI

Trong khi chạy, UI hiển thị từng phase `Students`, `Courses`, `Lessons`,
`Enrollments`, `Validation` với:

- số row đã xử lý và phần trăm;
- số row insert/đã có;
- tốc độ row/giây và ETA;
- tổng row và thời gian sau khi hoàn tất.

Mỗi batch dùng một parameterized multi-row insert và một transaction. Tool không
giữ toàn bộ dataset trong RAM.

## Safety và resume

Các điều kiện bắt buộc trước khi ghi:

- environment phải chính xác là `Development`;
- connection string phải trỏ tới `lms_student_db` và `lms_course_db`;
- phải có flag `--confirm`;
- migration `001` và các bảng đích phải tồn tại;
- chỉ một seed process được chạy nhờ MySQL advisory lock;
- mặc định các bảng `students`, `courses`, `lessons`, `enrollments` phải rỗng.

Nếu process bị hủy hoặc lỗi giữa chừng, chạy lại đúng options và random seed với
`--resume`:

```bash
scripts/seed/run-development-seed.sh \
  --confirm \
  --resume \
  --random-seed 20260813
```

Insert là idempotent theo deterministic UUID. `--resume` chỉ dành cho chính
dataset bị gián đoạn; không dùng nó để trộn dữ liệu thủ công hoặc random seed
khác. Final validation yêu cầu row count khớp chính xác kế hoạch.

## Reset toàn bộ seed data

Seed data nằm trong database thật của Course và Student Service, vì vậy reset
development database hiện có xóa toàn bộ seed data:

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

Lệnh reset drop/recreate cả năm MySQL database và migration history, sau đó API
apply lại `V001`. MinIO không bị thay đổi. Không cần và không có migration rollback
riêng cho seed.

## Chạy tool trực tiếp trên host

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

Xem mọi option:

```bash
dotnet run --project backend/Tools/Lms.DataSeeder/Lms.DataSeeder.csproj -- --help
```

Không log connection string hoặc password; bảng cấu hình chỉ hiển thị
server/port/database.

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

Với cấu hình mặc định, hai range phải lần lượt là `1-5` và `1-10`.

## Troubleshooting

- `target tables are not empty`: reset database, hoặc chỉ dùng `--resume` khi
  tiếp tục đúng dataset bị gián đoạn.
- `does not contain migration version 001`: khởi động lại Course/Student Service
  để apply migration.
- `Another development data seed process is already running`: đợi process hiện
  tại kết thúc; lock tự được MySQL giải phóng nếu connection đóng.
- `final row counts do not match`: dữ liệu hiện tại không thuộc đúng deterministic
  dataset; reset rồi seed lại.
- Hủy bằng `Ctrl+C`: batch đang chạy rollback; các batch đã commit được giữ để
  resume.
