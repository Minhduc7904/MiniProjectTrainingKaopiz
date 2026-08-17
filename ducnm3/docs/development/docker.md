# 37. Docker Compose

Các service:

```text
api-gateway
course-api
student-api
media-api
notification-api
scheduler-api
scheduler-worker
media-worker
notification-worker
mysql
minio
rabbitmq
```

Khuyến nghị 5 ngày:

```text
api-gateway
course-service
student-service
media-service
notification-service
scheduler-service
scheduler-worker
mysql
minio
rabbitmq
```

`scheduler-worker` chạy MassTransit host và kết nối RabbitMQ. Worker chưa có
polling/claim/execution loop hoặc consumer nghiệp vụ.

`media-worker` xử lý thumbnail và notification media usage; `notification-worker`
xử lý snapshot/dispatch notification batch. Hai worker này cùng dùng RabbitMQ và
database của service sở hữu, không mở HTTP port riêng.

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
scheduler-worker
mysql
minio
rabbitmq
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
rabbitmq_data
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

RABBITMQ_USER=
RABBITMQ_PASSWORD=
RABBITMQ_VHOST=/
MESSAGING_RETRY_COUNT=3
```

Không commit `.env` thật.

RabbitMQ AMQP mở ở `localhost:5672`; management UI ở
`http://localhost:15672`. Các API chỉ ready khi database và RabbitMQ healthy.
Retry, prefetch và concurrency của toàn bộ consumer được khai báo một lần qua
các biến `MESSAGING_*`.

---
