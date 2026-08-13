# 37. Docker Compose

Các service:

```text
api-gateway
course-api
student-api
media-api
notification-api
scheduler-api
mysql
minio
```

Khuyến nghị 5 ngày:

```text
api-gateway
course-service
student-service
media-service
notification-service
scheduler-service
mysql
minio
```

`SchedulerService.Worker` hiện mới là khung cơ bản và chưa được chạy thành container
cho tới khi có polling/claim/execution loop.

`data-seeder` là công cụ phát triển chạy một lần trong profile `seed`; nó không
chạy khi dùng `docker compose up` bình thường. Chỉ gọi qua
`scripts/seed/run-development-seed.sh --confirm`.

---
# 38. Mạng Docker

```text
lms-network
```

Các container gọi nhau bằng tên service:

```text
course-service
student-service
media-service
notification-service
scheduler-service
mysql
minio
```

Ví dụ:

```text
Server=mysql
Port=3306
```

Không dùng:

```text
localhost
```

để gọi MySQL từ container.

---
# 39. Volume Docker

```text
mysql_data
minio_data
```

Mục đích:

```text
docker compose down
```

không làm mất dữ liệu.

Nếu dùng:

```text
docker compose down -v
```

thì volume mới bị xóa.

---
# 40. Biến môi trường

`.env.example`:

```env
MYSQL_ROOT_PASSWORD=
MYSQL_DATABASE=
MYSQL_USER=
MYSQL_PASSWORD=

MINIO_ROOT_USER=
MINIO_ROOT_PASSWORD=

JWT_SECRET=
```

Không commit `.env` thật.

---
