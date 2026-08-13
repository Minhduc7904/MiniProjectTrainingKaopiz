# Giao tiếp giữa các service

## Quy tắc chọn transport

| Nhu cầu | Transport | Cách dùng |
| --- | --- | --- |
| QUERY cần kết quả ngay | HTTP | Typed client, chủ yếu dùng `GET` |
| COMMAND yêu cầu service khác thực hiện hành động, không chờ kết quả nghiệp vụ | RabbitMQ | `ICommandSender.SendAsync` |
| EVENT thông báo sự kiện đã xảy ra cho nhiều subscriber | RabbitMQ | `IEventPublisher.PublishAsync` |

Mỗi service vẫn sở hữu database riêng. Không dùng HTTP hoặc RabbitMQ để che giấu
việc truy vấn chéo database.

Foundation dùng MassTransit 8.x theo Apache 2.0. Không nâng lên MassTransit 9+
nếu chưa đánh giá và chấp thuận commercial license.

## QUERY qua HTTP

`BuildingBlocks.Http` cung cấp `AddServiceQueryClient<TClient, TImplementation>`.
Base URL được lấy từ `ServiceEndpoints:<service-name>`. Timeout và retry dùng
chung section `Communication:HttpQuery`.

- Chỉ retry các request idempotent; `POST`, `PUT`, `PATCH` và `DELETE` bị loại
  khỏi retry policy mặc định.
- `X-Correlation-Id` của request hiện tại được forward sang service đích.
- Client phải parse response envelope chuẩn và ánh xạ lỗi của dependency tại
  ranh giới Infrastructure.

## COMMAND qua RabbitMQ

Message command implement `ICommand` và được gửi trực tiếp đến queue của service
owner. Queue có format:

```text
<owner-service>--<command-type-kebab-case>
```

Consumer được đăng ký bằng `AddCommandConsumer<TConsumer, TCommand>`. Mỗi command
chỉ có một owner; không dùng `Publish` cho command.

## EVENT qua RabbitMQ

Integration event implement `IIntegrationEvent` và được publish theo message
type. Mỗi subscriber đăng ký queue riêng bằng
`AddEventConsumer<TConsumer, TEvent>`:

```text
<subscriber-service>--<event-type-kebab-case>
```

Cách này cho phép fan-out độc lập: một subscriber lỗi không chặn subscriber
khác.

## Retry tập trung và error queue

Mọi receive endpoint nhận cùng retry policy từ `AddLmsMessaging(...)`:

```text
Messaging:Retry:RetryCount
Messaging:Retry:InitialIntervalSeconds
Messaging:Retry:IntervalIncrementSeconds
```

Default là 3 lần retry theo incremental backoff. Consumer không tự khai báo hoặc
override retry. Sau khi hết retry, MassTransit chuyển message sang queue có hậu
tố `_error`; không requeue vô hạn. `prefetch` và concurrency cũng được quản lý
tập trung trong `Messaging:Consumer`.

## Correlation, delivery và idempotency

Publisher/sender thêm `X-Source-Service`, `X-Correlation-Id` và MassTransit
message metadata. Consumer phải log message type, attempt và correlation ID,
nhưng không log credential hoặc payload nhạy cảm.

RabbitMQ cung cấp delivery **at-least-once**. Vì vậy consumer phải idempotent,
thường bằng business key hoặc message ID. Foundation hiện chưa có transactional
Outbox/Inbox: nếu một use case vừa commit database vừa gửi message thì vẫn có
rủi ro dual-write. Phải bổ sung Outbox/Inbox trước khi dùng message cho luồng
nghiệp vụ quan trọng.

## Readiness

Mỗi service API chỉ trả `200` từ `/health` khi database và MassTransit bus đều
healthy. Media Service còn kiểm tra MinIO. Khi broker không sẵn sàng, endpoint
trả `503 DEPENDENCY_UNAVAILABLE`.

Scheduler Worker hiện khởi động MassTransit host và chờ COMMAND/EVENT contract
thật trong các giai đoạn sau; chưa có polling hoặc job handler nghiệp vụ.
