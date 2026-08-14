---
title: Tham chiếu unit test
description: Ranh giới, cấu trúc NUnit, test double và tiêu chuẩn deterministic cho unit test backend.
---

# Tham chiếu unit test

## 1. Ranh giới của unit test

Unit test chứng minh hành vi của một đơn vị logic trong process. Test phải chạy
được khi máy không có Docker, không có network và không có database.

| Phù hợp | Không phù hợp |
| --- | --- |
| Domain invariant, calculation, transition | HTTP route, middleware, response envelope |
| Application handler với dependency giả | ASP.NET Core pipeline qua `TestServer` |
| Validation, mapping, option policy | SQL migration hoặc ORM provider thật |
| Infrastructure naming/parsing deterministic | MySQL, MinIO, RabbitMQ hoặc service bên ngoài |
| Cancellation được dependency giả ghi nhận | Docker Compose hoặc Testcontainers |

Một class thuộc Infrastructure không tự động biến test thành integration test.
Nếu logic chỉ biến đổi input thành output và mọi I/O đã thay bằng interface giả,
nó vẫn là unit test. Ngược lại, gọi endpoint thật trong memory vẫn là component
test, không phải unit test.

## 2. Quy tắc chọn project

Xác định owner từ project chứa production type:

```text
backend/Services/Course/CourseService.Application/...
-> backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj

backend/BuildingBlocks/BuildingBlocks.Communication/...
-> backend/BuildingBlocks/BuildingBlocks.Communication.UnitTests/
   BuildingBlocks.Communication.UnitTests.csproj

backend/Tools/Lms.DataSeeder/...
-> backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj
```

Không đặt test vào project của consumer chỉ vì consumer kích hoạt behavior.
Không dùng project `*.IntegrationTests`, project chạy `TestServer`, hoặc
`BuildingBlocks.Presentation.Tests` cho unit test mới. Nếu repository có test
cũ trộn loại, không tiếp tục nhân rộng cách tổ chức đó.

Project NUnit tối thiểu cần `Microsoft.NET.Test.Sdk`, `NUnit`,
`NUnit3TestAdapter`, `NUnit.Analyzers`, `coverlet.collector` và
`ProjectReference` tới project sở hữu SUT. Dùng version đang có trong repository;
không tự đoán version mới.

## 3. Tính deterministic

Mỗi nguồn không xác định phải được kiểm soát:

- Thời gian: inject clock hoặc truyền timestamp cố định.
- UUID: inject generator hoặc tạo constant bằng `Guid.Parse`.
- Random: inject generator; nếu thuật toán cần random, dùng seed cố định.
- Environment/configuration: dựng configuration trong memory với giá trị rõ ràng.
- Culture/time zone: truyền culture/time zone hoặc đặt input không phụ thuộc máy.
- Async: await task; không dùng delay để hy vọng state đã đổi.
- Collection: không assert thứ tự nếu contract không quy định; nếu có quy định,
  assert thứ tự trực tiếp.
- Static mutable state: reset trong setup/teardown hoặc loại bỏ khỏi SUT.

Không lấy output không xác định của cùng lần chạy làm expected value. Ví dụ
`var id = Guid.NewGuid()` có thể dùng làm input được giữ lại để so sánh, nhưng
không phù hợp nếu mục tiêu là chứng minh thuật toán sinh ID cố định.

## 4. Arrange–Act–Assert

### Arrange

- Chỉ tạo dữ liệu liên quan tới scenario.
- Dùng builder/mother khi object hợp lệ cần nhiều field lặp lại.
- Đặt tên input theo vai trò, ví dụ `existingStudent`, `cancelledToken`.
- Cấu hình test double để ghi nhận call khi interaction là một phần contract.

### Act

- Một lời gọi hành vi chính.
- Với exception sync dùng `Assert.Throws<T>`.
- Với exception async dùng `Assert.ThrowsAsync<T>`.
- Không đặt assertion trong callback của fake nếu có thể ghi nhận rồi assert sau.

### Assert

- Assert output và state thuộc public contract.
- Chỉ assert interaction khi interaction là hành vi quan sát được quan trọng,
  ví dụ cancellation token phải được truyền tiếp hoặc repository không được gọi
  khi validation thất bại.
- Dùng `Assert.Multiple` cho nhiều field của cùng response.
- Không assert implementation detail như private method hoặc thứ tự nội bộ không
  được contract quy định.

## 5. Quy ước tên

```text
Class:  <Sut>Tests
Method: <Member>_<Scenario>_<ExpectedResult>
```

Ví dụ:

- `HandleAsync_StudentExists_ReturnsStudentDetails`
- `HandleAsync_StudentMissing_ThrowsStudentNotFound`
- `Validate_EmptyBucketName_ReturnsValidationError`
- `CheckAsync_TokenCancelled_ForwardsCancellation`

Tên phải cho reviewer biết behavior mà không cần đọc body. Với
`[TestCase]`/`[TestCaseSource]`, method vẫn mô tả một rule duy nhất; thêm
`.SetName(...)` nếu case mặc định khó đọc.

## 6. Reusable test doubles

Ưu tiên test double nhỏ, explicit và không có framework behavior ẩn:

- `Stub`: trả dữ liệu cố định.
- `Fake`: implementation nhẹ trong memory có state.
- `Spy`: ghi nhận lời gọi/argument.
- `Builder`: tạo object hợp lệ, cho phép override field liên quan.

Đặt double dùng bởi từ hai fixture trong:

```text
<UnitTests project>/
├── TestDoubles/
│   ├── StubClock.cs
│   ├── SpyStudentRepository.cs
│   └── TestDataBuilder.cs
└── <Area>/
    └── <Sut>Tests.cs
```

Test double phải:

- Implement đúng interface production.
- Có default an toàn và được reset hoặc tạo mới mỗi test.
- Không tự gọi network, filesystem, database hoặc timer.
- Không chứa assertion; expose state để fixture assert.
- Tôn trọng `CancellationToken` nếu behavior cần kiểm tra cancellation.

Private nested stub chỉ phù hợp khi một fixture duy nhất dùng và implementation
ngắn. Khi xuất hiện bản sao thứ hai, chuyển nó vào `TestDoubles/`.

## 7. Ma trận coverage

Chọn case theo contract, không chạy theo coverage line:

1. Happy path với input đại diện.
2. Boundary value: rỗng, min/max, duplicate hoặc transition biên.
3. Expected business error và đúng error code.
4. Dependency trả `null`, not found hoặc trạng thái không hợp lệ.
5. Cancellation được tôn trọng.
6. Interaction quan trọng: không gọi dependency sau validation failure; gọi đúng
   một lần khi idempotency yêu cầu.

Không thêm mọi case vào mọi test. Chỉ chọn case có thể thay đổi kết luận về
behavior.

## 8. Catalog tài liệu

Trang `docs/tests/<owner>/unit.md` cần ghi:

- Đường dẫn project `.csproj`.
- File/class/method test thực sự tồn tại.
- SUT và dependency giả được dùng.
- Input hoặc trạng thái ban đầu.
- Action.
- Kết quả và error code chính xác.
- Lệnh chạy project.

Nếu `unit.md` mới được tạo, thêm link vào `docs/tests/README.md`. Catalog chỉ mô
tả test đã có trong source; không ghi planned coverage như đã hoàn thành.

## 9. Verification và xử lý lỗi

Thứ tự khuyến nghị:

```bash
dotnet test <exact-project-path>.csproj --filter "FullyQualifiedName~<Sut>Tests"
dotnet test <exact-project-path>.csproj
dotnet format <exact-project-path>.csproj --verify-no-changes
```

Chạy `dotnet test backend/Lms.sln -m:1` khi thay shared production code,
`Directory.*` hoặc project reference.

Khi test flaky:

1. Xác nhận source không xác định: clock, static state, race, random hoặc order.
2. Sửa fixture/SUT để kiểm soát source đó.
3. Chạy focused test nhiều lần.
4. Không chữa flaky bằng retry mù, timeout dài hoặc `[NonParallelizable]` nếu
   test vốn phải độc lập.

## 10. Anti-pattern

- Gọi localhost, SDK cloud hoặc HTTP thật.
- Dùng EF provider khác để giả lập semantics MySQL.
- Mở container trong unit fixture.
- Dùng `Thread.Sleep`/`Task.Delay` như synchronization.
- Một test kiểm tra nhiều business rule không liên quan.
- Copy-paste fake lớn giữa fixture.
- Đổi production visibility chỉ để test private implementation.
- Đặt endpoint test trong `*.UnitTests` vì nó chạy nhanh.
