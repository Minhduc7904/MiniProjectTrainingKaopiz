# Hướng dẫn Database First và migration SQL

## Kiến trúc và nguồn dữ liệu chuẩn

Dự án sử dụng MySQL theo hướng Database First:

```text
migration SQL có phiên bản
    -> schema MySQL
    -> sinh mã EF Core
    -> DbContext và các mô hình lưu trữ được sinh
```

Migration SQL là nguồn dữ liệu chuẩn duy nhất của schema. Không dùng `dotnet ef migrations add`, `dotnet ef database update` hoặc `Database.Migrate()` của EF khi chạy để quản lý schema.

Mỗi dịch vụ sở hữu một cơ sở dữ liệu và một thư mục migration:

```text
Course Service       lms_course_db        Services/Course/CourseService.Infrastructure/Database/Migrations/
Student Service      lms_student_db       Services/Student/StudentService.Infrastructure/Database/Migrations/
Media Service        lms_media_db         Services/Media/MediaService.Infrastructure/Database/Migrations/
Notification Service lms_notification_db  Services/Notification/NotificationService.Infrastructure/Database/Migrations/
Scheduler Service    lms_scheduler_db     Services/Scheduler/SchedulerService.Infrastructure/Database/Migrations/
```

Không có khóa ngoại xuyên cơ sở dữ liệu dịch vụ. Lưu các ID bên ngoài như `student_id` dưới dạng giá trị vô hướng và kiểm tra chúng qua hợp đồng dịch vụ khi cần.

## Thông tin xác thực và biến môi trường

`.env` chỉ dùng cục bộ và được Git bỏ qua. Bắt đầu từ `.env.example`:

```bash
cp .env.example .env
```

Đặt mật khẩu cục bộ thực trong `.env`. Tuyệt đối không đặt mật khẩu, thông tin xác thực root hoặc chuỗi kết nối trong mã nguồn hay `appsettings.json`.

Đặt giá trị chuỗi kết nối trong `.env` giữa dấu nháy vì `User ID` chứa khoảng trắng và tệp này cũng được quy trình tự động hóa dựa trên shell nạp.

Mỗi API chỉ nhận biến môi trường `ConnectionStrings__Database` của chính nó. Docker Compose chuyển tiếp chuỗi kết nối khi chạy tương ứng từ `.env`. Quy trình tự động hóa thủ công dùng:

- `COURSE_DB_LOCAL_CONNECTION_STRING`
- `STUDENT_DB_LOCAL_CONNECTION_STRING`
- `MEDIA_DB_LOCAL_CONNECTION_STRING`
- `NOTIFICATION_DB_LOCAL_CONNECTION_STRING`
- `SCHEDULER_DB_LOCAL_CONNECTION_STRING`

Các biến thể cục bộ dùng `Server=localhost` cho lệnh migration và sinh mã chạy trên máy chủ. Các biến thể khi chạy trong Docker dùng `Server=mysql`.

## Lịch sử migration và hành vi khởi động

Trước khi áp dụng migration của dịch vụ, ứng dụng tạo bảng kỹ thuật sau trong cơ sở dữ liệu của mình:

```text
schema_migrations
├── version
├── name
├── applied_at
└── checksum
```

Khi khởi động, mỗi API:

1. Đợi MySQL và container chạy một lần `mysql-init`.
2. Lấy khóa migration MySQL cho cơ sở dữ liệu của mình.
3. Tạo `schema_migrations` nếu bảng chưa tồn tại.
4. Đọc các tệp SQL theo thứ tự phiên bản.
5. Xác minh checksum của các migration đã áp dụng trước đó.
6. Áp dụng từng migration đang chờ, ghi bản ghi lịch sử và ghi nhật ký phiên bản.
7. Dừng khởi động khi migration, kiểm tra checksum hoặc ghi lịch sử thất bại.

Lỗi không bao giờ bị bỏ qua âm thầm. DDL của MySQL có thể commit ngầm, vì vậy một migration DDL thất bại có thể cần migration sửa lỗi tiếp theo hoặc thao tác đặt lại môi trường phát triển thủ công.

## Quy ước đặt tên migration

Dùng:

```text
V<zero-padded-version>__<lowercase-description>.sql
```

Ví dụ:

```text
V001__create_learning_tables.sql
V002__add_course_status_index.sql
V003__add_description_markdown.sql
```

Quy tắc:

- Phiên bản là duy nhất và tăng dần trong một dịch vụ.
- Không bao giờ sửa migration đã áp dụng. Checksum của migration được kiểm tra khi khởi động.
- Dùng chữ thường, chữ số, dấu gạch ngang và dấu gạch dưới sau `__`.
- Một migration chỉ nên thực hiện một thay đổi schema nhất quán.

## Tạo và áp dụng migration

Tạo tệp SQL trong thư mục migration của dịch vụ sở hữu, ví dụ:

```bash
touch backend/Services/Course/CourseService.Infrastructure/Database/Migrations/V002__add_course_slug.sql
```

Viết SQL định nghĩa schema tại đó. Sau đó chạy migration của dịch vụ ở môi trường cục bộ:

```bash
set -a
. ./.env
set +a
sh scripts/database/tools/migrate.sh course
```

Migration này tự động chạy trước khi API khởi động trong Docker.

## Thêm thay đổi schema

### Thêm bảng

Tạo migration mới:

```sql
CREATE TABLE courses (
    id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL,
    created_at DATETIME NOT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB;
```

### Thêm cột

Tạo migration tiếp theo; không sửa migration tạo bảng sau khi migration đó đã được áp dụng:

```sql
ALTER TABLE courses
    ADD COLUMN description_markdown TEXT NULL;
```

### Thêm chỉ mục

Dùng một migration riêng và tên có tính mô tả:

```sql
CREATE INDEX ix_courses_created_at ON courses (created_at);
```

## Hoàn tác và migration thất bại

Môi trường sản xuất chỉ dùng migration tiến. Ưu tiên migration sửa lỗi mới thay vì hoàn tác.

Đối với migration thất bại:

1. Dừng lại và kiểm tra phiên bản migration cùng lỗi MySQL trong nhật ký.
2. Kiểm tra xem MySQL đã áp dụng DDL nào trước khi xảy ra lỗi hay chưa.
3. Trong môi trường dùng chung hoặc sản xuất, tạo migration tiến mới để sửa schema.
4. Trong cơ sở dữ liệu phát triển cục bộ có thể xóa bỏ, đặt lại cơ sở dữ liệu và chạy lại migration nếu phù hợp.

Không xóa hoặc sửa bản ghi trong `schema_migrations` chỉ để chạy lại migration, trừ khi schema thực tế đã được đặt lại cho khớp.

## Đặt lại toàn bộ cơ sở dữ liệu phát triển

Đường cơ sở sạch hiện tại chứa chính xác một bản ghi `V001` trong mỗi cơ sở dữ
liệu dịch vụ. Khi thay đổi đường cơ sở này trong môi trường phát triển có thể xóa
bỏ, không chỉ xóa các bản ghi `schema_migrations`: những bảng được giữ lại sẽ
không còn khớp với lịch sử.

Dùng thao tác đặt lại có cơ chế bảo vệ:

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

Tập lệnh từ chối chạy nếu thiếu `--confirm`, nếu không ở
`ASPNETCORE_ENVIRONMENT=Development`, hoặc nếu tên cơ sở dữ liệu khác năm tên cục
bộ dự kiến. Tập lệnh dừng các API, chỉ xóa/tạo lại năm cơ sở dữ liệu MySQL và người
dùng qua `mysql-init`, đồng thời giữ nguyên volume MinIO. Sau đó, mỗi API tạo
`schema_migrations` và áp dụng `V001` sạch của mình.

Chỉ dùng `docker compose down -v` khi chủ ý xóa cả dữ liệu phát triển MySQL và
MinIO.

## Sinh mã EF Core theo hướng Database First

Chỉ sinh mã sau khi migration SQL thành công. Sinh mã là thao tác phát triển, tuyệt đối không phải thao tác khởi động môi trường sản xuất.

```bash
set -a
. ./.env
set +a
sh scripts/database/tools/scaffold.sh course
```

Thay `course` bằng `student`, `media`, `notification` hoặc `scheduler`.

Lệnh sử dụng:

```text
dotnet ef dbcontext scaffold
Pomelo.EntityFrameworkCore.MySql
--no-onconfiguring
--no-build
--force
```

Quy trình tự động hóa chỉ chọn bảng nghiệp vụ của dịch vụ; không sinh mã cho bảng kỹ thuật `schema_migrations`.
`--no-build` cho phép sinh lại mã ngay cả khi mã nguồn được sinh hiện tại chưa khớp schema; luôn chạy `dotnet build` ngay sau khi sinh mã.

Mã được sinh chỉ thuộc về dự án Infrastructure của dịch vụ tương ứng:

```text
Infrastructure/
└── Persistence/
    ├── <Service>DbContext.cs
    └── Scaffolded/
```

Không bao giờ sinh mã từ cơ sở dữ liệu của dịch vụ khác. Ví dụ, Student Service không bao giờ sinh mã từ `lms_course_db`.

## Sinh lại mã an toàn

`DbContext` và các thực thể được sinh tự động:

- Không đặt logic nghiệp vụ trong các tệp được sinh.
- Dùng lớp từng phần để mở rộng mô hình được sinh.
- Dùng bộ ánh xạ để chuyển mô hình lưu trữ thành thực thể Domain.
- Giữ các ca sử dụng và giao diện trong Application.
- Sinh lại mã với `--force` sau mỗi migration schema đã được phê duyệt.

Rà soát phần khác biệt được sinh sau mỗi lần sinh mã để bảo đảm chỉ dịch vụ sở hữu thay đổi.

## Quy trình Docker

```bash
docker compose up -d --build
```

Thứ tự phụ thuộc khi khởi động:

```text
mysql khỏe mạnh
    -> mysql-init tạo cơ sở dữ liệu và người dùng
    -> dịch vụ áp dụng migration SQL
    -> dịch vụ khởi động
```

Trong môi trường sản xuất, dùng cùng thứ tự nhưng truyền thông tin xác thực thực qua kho bí mật của hệ thống triển khai. Không sinh mã trong môi trường sản xuất.

## Quy trình phát triển hằng ngày

1. Cập nhật `.env` cục bộ từ `.env.example`.
2. Khởi động MySQL và các dịch vụ bằng Docker Compose.
3. Thêm migration SQL có phiên bản cho các thay đổi schema.
4. Áp dụng và kiểm tra migration.
5. Chỉ sinh mã từ cơ sở dữ liệu của dịch vụ sở hữu.
6. Triển khai ánh xạ lưu trữ trong Infrastructure và hành vi nghiệp vụ trong Domain/Application.
7. Dựng mã và chạy kiểm thử.
8. Commit migration SQL, các tệp được sinh khi áp dụng, `.env.example` và bản cập nhật hướng dẫn; tuyệt đối không commit `.env`.
