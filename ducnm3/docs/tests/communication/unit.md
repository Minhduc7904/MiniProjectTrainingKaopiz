# Communication foundation - unit tests

Project:

```text
backend/BuildingBlocks/BuildingBlocks.Communication.UnitTests
```

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Communication.UnitTests/BuildingBlocks.Communication.UnitTests.csproj
```

## Test cases

### MessageEndpointNamesAreDeterministic

- Input: service owner/subscriber và CLR message type.
- Pass khi command queue và event subscriber queue được đổi sang kebab-case
  đúng convention, không phụ thuộc runtime.

### MessagingOptionsRejectInvalidCentralRetryConfiguration

- Input: `Messaging:Retry:RetryCount` âm.
- Pass khi options validation fail-fast và chỉ rõ field retry không hợp lệ.

### AddLmsMessagingRegistersTheSingleCentralRetryConfiguration

- Input: một section `Messaging:Retry` với retry count và interval tùy chỉnh.
- Pass khi DI chỉ resolve bộ `MessagingOptions` trung tâm với đúng các giá trị
  đã bind; consumer không cần khai báo retry riêng.

### SendAsyncForwardsCurrentHttpCorrelationId

- Input: request context có `TraceIdentifier`.
- Pass khi outbound HTTP request chứa cùng `X-Correlation-Id`.

### GetRetriesTransientFailureButPostDoesNot

- GET nhận hai response `503`, sau đó `200`: pass khi handler được gọi ba lần và
  response cuối thành công.
- POST nhận `503`: pass khi handler chỉ được gọi một lần, chứng minh QUERY retry
  không retry unsafe HTTP method.
