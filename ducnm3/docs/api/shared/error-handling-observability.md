# Xử lý lỗi và khả năng quan sát

## Xử lý lỗi

Sử dụng `ExceptionHandlingMiddleware` toàn cục và hợp đồng phản hồi dùng chung trong [error-format.md](error-format.md).

```text
ValidationException
    → 400

NotFoundException
    → 404

ConflictException
    → 409

Request body hoặc multipart vượt giới hạn
    → 413 PAYLOAD_TOO_LARGE

Ngoại lệ chưa được xử lý
    → 500

Cơ sở dữ liệu không khả dụng
    → 503 DATABASE_UNAVAILABLE

Kho lưu trữ phương tiện không khả dụng
    → 503 STORAGE_UNAVAILABLE

Cơ sở dữ liệu và kho lưu trữ phương tiện không khả dụng
    → 503 DEPENDENCY_UNAVAILABLE

RabbitMQ/MassTransit bus không khả dụng
    → 503 DEPENDENCY_UNAVAILABLE

Dịch vụ hạ nguồn không khả dụng tại API Gateway
    → 503 SERVICE_UNAVAILABLE
```

Tuyệt đối không làm lộ dấu vết ngăn xếp, chuỗi kết nối, SQL nội bộ hoặc thông tin bí mật.

## Ghi nhật ký và khả năng quan sát

Sử dụng nhật ký có cấu trúc (ví dụ: Serilog) với các thông tin sau:

```text
Yêu cầu
Trạng thái phản hồi
Thời gian thực thi
CorrelationId

Bắt đầu tác vụ theo lô
Số thứ tự lô
Kích thước lô
Số lượng thành công
Số lượng thất bại
Thử lại
Thời gian thực thi

Thao tác cơ sở dữ liệu chậm
Ngoại lệ chưa được xử lý
```

Ví dụ:

```text
JobId=123
Batch=15
Records=500
Success=493
Failed=7
ElapsedMs=836
```
