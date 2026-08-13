---
title: Tham chiếu integration test
description: Ranh giới Testcontainers, migration thật, isolation và lifecycle cho MySQL, MinIO, RabbitMQ.
---

# Tham chiếu integration test

## 1. Mục tiêu và ranh giới

Integration test chứng minh production adapter hoạt động đúng với implementation
thật của dependency. Chi phí container chỉ hợp lý khi test cần semantics mà fake
không thể đại diện.

| Boundary | Hành vi cần resource thật |
| --- | --- |
| MySQL | Migration, type mapping, index/constraint, transaction, isolation |
| MinIO | Bucket/object API, metadata, stream, not-found và delete lifecycle |
| RabbitMQ | Exchange/queue topology, routing, retry, error queue, delivery |
| HTTP adapter | Serialization/client policy với server cô lập do fixture sở hữu |

Endpoint/middleware với dependency giả thuộc component test. Business rule với
interface giả thuộc unit test. Không mở container chỉ để tăng nhãn
“integration”.

## 2. Chọn project theo owner

```text
backend/Services/Media/MediaService.Infrastructure/...
-> backend/Services/Media/MediaService.IntegrationTests/
   MediaService.IntegrationTests.csproj

backend/BuildingBlocks/BuildingBlocks.Messaging/...
-> backend/BuildingBlocks/BuildingBlocks.Messaging.IntegrationTests/
   BuildingBlocks.Messaging.IntegrationTests.csproj

backend/Tools/Lms.DataSeeder/...
-> backend/Tools/Lms.DataSeeder.IntegrationTests/
   Lms.DataSeeder.IntegrationTests.csproj
```

Luồng thật sự sở hữu bởi nhiều service đặt ở
`tests/<Flow>.IntegrationTests/<Flow>.IntegrationTests.csproj`. Không đặt test
cross-service vào service tình cờ là caller.

Không dùng `*.UnitTests`, `*.ComponentTests` hoặc project test đa loại. Dùng
package Testcontainers chuyên biệt phù hợp (`Testcontainers.MySql`,
`Testcontainers.Minio`, `Testcontainers.RabbitMq`) với version đã được repository
sử dụng; không tự đoán version.

## 3. Nguyên tắc cô lập tuyệt đối

Fixture phải tự sở hữu mọi resource:

- Container được build/start từ test code.
- Host, mapped port và connection string lấy từ container.
- Credential chỉ dùng cho test, không đọc developer secret.
- Database/schema/bucket/prefix/queue có identity riêng.
- Data được tạo trong test hoặc setup của chính fixture.
- Teardown dispose container và client.

Bị cấm:

- `docker compose up` hoặc connection string của Compose.
- `localhost:3306`, `localhost:9000`, `localhost:5672` giả định.
- Database, bucket, exchange hoặc queue dùng chung giữa developer/test run.
- Resource tồn tại sẵn ngoài fixture.
- Reuse container toàn máy nếu làm test phụ thuộc run khác.

Mapped port vẫn là network tới container nhưng được phép vì resource cô lập do
fixture sở hữu. External network và service không do fixture kiểm soát vẫn bị cấm.

## 4. Lifecycle

### Fixture-level container

Dùng `[OneTimeSetUp]`/`[OneTimeTearDown]` khi startup tốn thời gian và mỗi test có
thể reset state an toàn:

1. Build container.
2. Start container.
3. Tạo production client/connection factory.
4. Apply migration và bootstrap resource.
5. Trước mỗi test, tạo isolation key hoặc reset data sở hữu bởi test.
6. Sau mỗi test, dọn object/row/queue theo key.
7. Sau suite, dispose client và container.

### Test-level container

Dùng container riêng mỗi test khi:

- Test migration từ database trống hoặc version khác nhau.
- Test destructive startup/failure behavior.
- State không thể reset đáng tin cậy.

Luôn giữ reference container ngay sau khi build để teardown có thể dispose nếu
startup/bootstrap thất bại. Cleanup quan trọng nên nằm trong `try/finally`.

`[NonParallelizable]` phù hợp khi fixture dùng một broker/database chung nội bộ
và semantics không cho chạy song song. Nó không hợp lệ để che việc dùng resource
developer/shared.

## 5. MySQL và migration thật

### Áp dụng migration

- Dùng migration runner production.
- Dùng chính file `*.sql` production theo đúng thứ tự/version.
- Nếu test project cần file runtime, link chúng trong `.csproj` với
  `CopyToOutputDirectory`.
- Bắt đầu từ database trống khi kiểm tra migration bootstrap.
- Fail test ngay nếu migration lỗi; không tiếp tục với schema nửa vời.

Không dùng:

- `EnsureCreated`.
- SQL schema copy trong fixture.
- SQLite/EF in-memory thay MySQL.
- Một subset migration chỉ đủ cho test pass.

### Repository test

Assert behavior relational có giá trị:

- Column/type/nullability mapping.
- Unique/foreign-key constraint.
- Query/filter/order/pagination.
- Transaction commit/rollback.
- Concurrency hoặc active-row uniqueness.
- Cancellation propagation khi provider hỗ trợ.

Dùng ID cố định hoặc isolation prefix. Nếu suite chia sẻ database, cleanup theo
foreign-key order hoặc transaction phù hợp; không xóa dữ liệu không sở hữu.

### Migration test

Các dạng cần thiết:

- Fresh database áp dụng toàn bộ migration.
- Upgrade từ version được hỗ trợ khi compatibility là contract.
- Constraint/index tồn tại và thực sự enforce.
- Migration idempotency chỉ khi runner contract yêu cầu.

## 6. MinIO

Container dùng image tag rõ ràng. Bootstrap:

1. Start MinIO container.
2. Tạo client từ endpoint/credential của container.
3. Tạo bucket production cần cho scenario hoặc bucket có suffix run riêng.
4. Khởi tạo production storage adapter.

Test adapter nên bao phủ contract:

- Upload stream và content type.
- Object tồn tại.
- Metadata/size/checksum.
- Download bytes.
- Delete và not-found behavior.
- Mapping category sang bucket/object key.
- Cancellation khi có thể quan sát.

Object key phải có prefix duy nhất theo test. Sau test, xóa object; sau suite,
xóa bucket nếu client yêu cầu rồi dispose container. Không dùng MinIO Compose của
developer.

## 7. RabbitMQ và communication

Container RabbitMQ phải do fixture tạo. Configuration production lấy từ:

- `rabbitMq.Hostname`
- `rabbitMq.GetMappedPublicPort(5672)`
- Username/password test
- Virtual host test nếu fixture tạo riêng

Mỗi fixture/run dùng endpoint/queue/subscriber identity không xung đột. Kiểm tra
đúng boundary:

- COMMAND tới đúng queue/consumer.
- EVENT fan-out tới từng subscriber.
- Retry count và interval theo configuration.
- Message lỗi đi vào `_error` queue.
- Correlation header được propagate.
- Bus/health readiness.

Không dùng `Task.Delay` cố định để đoán message đã tới. Dùng
`TaskCompletionSource` với `RunContinuationsAsynchronously`, polling có
`CancellationTokenSource(timeout)` hoặc harness signal. Mọi wait phải bounded và
thất bại với thông tin đủ chẩn đoán.

Reset state ghi nhận giữa các test. Stop host/bus trước khi dispose container.

## 8. Nhiều container trong một flow

Chỉ start resource cần thiết. Ví dụ flow Media cần MySQL + MinIO nhưng Student
lookup có thể là stub nếu mục tiêu không phải giao tiếp Media → Student.

Thứ tự:

1. Build tất cả container.
2. Start độc lập; có thể song song nếu không phụ thuộc.
3. Apply migration trước khi tạo repository.
4. Bootstrap bucket/queue.
5. Dựng production adapter/application.
6. Chạy test.
7. Stop host/client trước.
8. Dispose container theo thứ tự ngược.

Nếu một container start lỗi, dispose các container đã start. Không để failure
path bỏ qua cleanup.

## 9. Tính deterministic

- Pin image tag; không dùng `latest`.
- Dữ liệu, clock và UUID cần ổn định hoặc được giữ làm input.
- Không phụ thuộc thứ tự test.
- Timeout rõ ràng và đủ cho CI, nhưng không che deadlock.
- Không assert mapped port cụ thể.
- Không assert text lỗi từ engine nếu contract chỉ ổn định error type/code.
- Không chạy test song song trên cùng row/object/queue identity.

## 10. Quy ước tên

```text
Class:  <Boundary>IntegrationTests
Method: <Operation>_<Scenario>_<ExpectedResult>
```

Ví dụ:

- `ApplyAsync_EmptyDatabase_CreatesExpectedSchema`
- `SaveAsync_DuplicateActiveUsage_EnforcesUniqueConstraint`
- `UploadAsync_ValidStream_PersistsBytesAndMetadata`
- `PublishAsync_TwoSubscribers_DeliversToBothQueues`
- `ConsumeAsync_HandlerFails_RetriesThenMovesToErrorQueue`

## 11. Catalog tài liệu

`docs/tests/<owner>/integration.md` cần ghi:

- Exact `.csproj` và source fixture.
- Container/image và production adapter.
- Migration files/runner được áp dụng.
- Isolation và lifecycle/cleanup.
- Input/action.
- Expected database/object/message state.
- Docker prerequisite và lệnh chạy.

Thêm link vào `docs/tests/README.md` khi tạo catalog. Không đưa component endpoint
coverage hoặc unit logic vào trang integration.

## 12. Verification

```bash
docker info
dotnet test <exact-integration-project>.csproj --filter "FullyQualifiedName~<FixtureName>"
dotnet test <exact-integration-project>.csproj
dotnet format <exact-integration-project>.csproj --verify-no-changes
```

Chạy `dotnet test backend/Lms.sln -m:1` khi đổi shared migration runner,
messaging/storage adapter, package/reference hoặc solution.

Sau run, kiểm tra log test không cho thấy endpoint Compose/shared. Testcontainers
có thể dùng Ryuk để cleanup; fixture vẫn phải dispose rõ ràng, không dựa duy nhất
vào process exit.

## 13. Anti-pattern

- Dùng developer Compose vì “đã chạy sẵn”.
- Hard-code localhost port hoặc credential môi trường.
- Dùng schema tự viết thay migration production.
- Một fixture start cả stack dù chỉ test một adapter.
- Sleep dài để chờ message.
- Không dispose container/client/host.
- Test phụ thuộc data từ test trước.
- Dùng một bucket/queue tĩnh giữa parallel run.
- Gắn nhãn integration cho TestServer-only test.
