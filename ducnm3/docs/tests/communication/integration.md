# Messaging foundation - integration tests

Project:

```text
backend/BuildingBlocks/BuildingBlocks.Messaging.IntegrationTests
```

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Messaging.IntegrationTests/BuildingBlocks.Messaging.IntegrationTests.csproj
```

Test dùng RabbitMQ 4.1 management Testcontainer cô lập. Không đọc hoặc ghi
RabbitMQ của Docker Compose trên máy developer.

## CommandEventRetryAndCorrelationUseSharedTopology

Test case này kiểm tra cùng một runtime foundation:

1. COMMAND được `Send` đúng queue của Notification owner và chỉ consumer đó xử
   lý.
2. EVENT được `Publish` đến hai subscriber queue độc lập; cả hai đều nhận
   message.
3. Consumer cố ý throw exception. Với retry count bằng 2, pass khi tổng số lần
   consume đúng bằng 3.
4. Sau lần retry cuối, pass khi RabbitMQ có queue hậu tố `_error` chứa message;
   message không requeue vô hạn.
5. COMMAND, EVENT và các retry attempt giữ nguyên `X-Correlation-Id`.
6. `IMessagingHealthProbe` trả healthy sau khi MassTransit bus kết nối broker.
7. Worker observer ghi `Received`/`Completed` cho command thành công và ghi
   `Failed` có exception khi consumer cố ý throw sau retry policy. Các log được
   kiểm tra có `MessageId`, `CorrelationId`, queue, source service,
   `RetryAttempt` và `RetryLimit`, không cần inspect payload.

Test fail nếu Docker Engine không chạy, container không start được, topology sai,
fan-out thiếu subscriber, retry lệch cấu hình, correlation bị mất hoặc error
queue không nhận message.
