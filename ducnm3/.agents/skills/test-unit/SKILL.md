---
name: test-unit
description: Thêm hoặc cập nhật NUnit unit test cho logic Domain, Application và Infrastructure có tính xác định.
---

# Unit test

## Khi nào dùng

Dùng skill này khi cần kiểm tra một đơn vị logic chạy hoàn toàn trong process:

- Domain rule, value object hoặc domain service.
- Application handler, validator, mapper hoặc orchestration có dependency thay thế được.
- Infrastructure logic có kết quả xác định như option validation, naming, mapping hoặc policy.

Không dùng skill này cho HTTP endpoint/middleware chạy bằng `TestServer`, SQL
migration, repository với database thật, MinIO, RabbitMQ, Docker hoặc network.

## Cổng bắt buộc trước khi chỉnh sửa

Trước khi sửa bất kỳ file nào:

1. Đọc toàn bộ `.agents/skills/test-unit/reference.md`.
2. Đọc toàn bộ `.agents/skills/test-unit/template.md`.
3. Đọc `rules/testing.md`, `rules/code-quality.md`,
   `rules/documentation-language.md` và `docs/development/testing.md`.
4. Đọc implementation, test hiện có và trang `docs/tests/<owner>/unit.md` liên quan.
5. Chốt một đường dẫn project cụ thể theo mục bên dưới. Không bắt đầu chỉnh sửa
   khi vẫn còn placeholder như `<Service>`, `<Component>` hoặc `<Tool>`.

## Chọn chính xác project

Chọn theo component sở hữu SUT, không chọn theo component gọi nó:

- Service:
  `backend/Services/<Service>/<Service>Service.UnitTests/<Service>Service.UnitTests.csproj`.
- Building block:
  `backend/BuildingBlocks/<Component>.UnitTests/<Component>.UnitTests.csproj`.
- Tool:
  `backend/Tools/<Tool>.UnitTests/<Tool>.UnitTests.csproj`.

Nếu project chính xác đã tồn tại, thêm test vào đó. Nếu chưa tồn tại, chỉ tạo
project khi có hành vi thật cần kiểm tra và đặt nó đúng mẫu trên. Không đặt
component test hoặc integration test vào project `*.UnitTests`.

Trước khi chỉnh sửa, ghi lại trong kế hoạch làm việc:

```text
SUT: <namespace.type/member>
Project test: <đường dẫn .csproj chính xác>
File test: <đường dẫn .cs chính xác>
Catalog: docs/tests/<owner>/unit.md
```

## Quy trình

1. Xác định một hành vi quan sát được và các nhánh success/error của SUT.
2. Cố định clock, ID, random, configuration và input; không phụ thuộc thời gian
   máy, thứ tự test hoặc state dùng chung.
3. Thay dependency qua interface bằng reusable test double. Đặt double dùng
   chung trong `TestDoubles/`; chỉ giữ private fake trong test khi thật sự chỉ
   dùng một lần.
4. Viết NUnit test theo Arrange–Act–Assert. Mỗi test có một lý do thất bại rõ
   ràng; dùng `Assert.Multiple` cho nhiều thuộc tính của cùng một kết quả.
5. Đặt class là `<Sut>Tests` và method là
   `<Member>_<Scenario>_<ExpectedResult>`.
6. Bao phủ success, expected business error, validation và cancellation khi
   contract có các hành vi này.
7. Cập nhật `docs/tests/<owner>/unit.md` với project, file nguồn, dependency,
   input/action và điều kiện đạt. Nếu tạo catalog mới, thêm link vào
   `docs/tests/README.md`.
8. Chạy verification trước khi bàn giao.

## Lệnh

Chạy project chính xác đã chốt:

```bash
dotnet test <exact-project-path>.csproj
```

Lọc nhanh trong lúc phát triển:

```bash
dotnet test <exact-project-path>.csproj --filter "FullyQualifiedName~<Sut>Tests"
```

Kiểm tra toàn backend khi phạm vi thay đổi có thể ảnh hưởng component khác:

```bash
dotnet test backend/Lms.sln -m:1
```

Kiểm tra format mà không tự sửa:

```bash
dotnet format <exact-project-path>.csproj --verify-no-changes
```

## Verification bắt buộc

- Tất cả test mới và test liên quan đều pass.
- Chạy lặp lại focused test ít nhất hai lần khi test từng flaky hoặc có
  cancellation/concurrency.
- Không có network call, Docker, database thật hoặc filesystem state dùng chung.
- Test không phụ thuộc `DateTime.Now`, `Guid.NewGuid()` trong expected value,
  random không seed, thứ tự chạy hoặc test khác.
- Test double reusable không bị sao chép giữa nhiều fixture.
- Tên test mô tả scenario và kết quả, không dùng tên chung như `Works` hoặc
  `Test1`.
- `docs/tests/<owner>/unit.md` và `docs/tests/README.md` phản ánh đúng test thực
  tế, không ghi coverage chưa tồn tại.
- Diff chỉ chứa unit test, test support cần thiết và tài liệu liên quan; không
  trộn component/integration test.

## Điều kiện dừng

Nếu hành vi cần `TestServer`, chuyển sang `.agents/skills/test-component`. Nếu cần
MySQL, MinIO, RabbitMQ hoặc migration thật, chuyển sang
`.agents/skills/test-integration`; không nới lỏng ranh giới của unit test.
