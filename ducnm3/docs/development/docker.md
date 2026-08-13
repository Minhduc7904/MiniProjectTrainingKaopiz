# 37. Docker Compose

Services:

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

`SchedulerService.Worker` hiện là skeleton và chưa được chạy thành container
cho tới khi có polling/claim/execution loop.

---
# 38. Docker Network

```text
lms-network
```

Container gọi nhau bằng service name:

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
# 39. Docker Volume

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
# 40. Environment Variables

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
