# `GET /health`

## Mục đích

Kiểm tra Media Service có thể kết nối đến cơ sở dữ liệu do mình sở hữu và MinIO có
đủ cả năm vùng lưu trữ phương tiện đã cấu hình.

## Xác thực và phân quyền

- Xác thực: không bắt buộc.
- Vai trò/phạm vi: không có.
- Quy tắc sở hữu: không áp dụng.

## Yêu cầu

Không có tham số đường dẫn, tham số truy vấn hoặc nội dung yêu cầu. Không áp dụng phân trang.

## Phản hồi thành công

```http
200 OK
```

```json
{
  "data": {
    "service": "media-service",
    "status": "healthy",
    "database": {
      "status": "healthy"
    },
    "storage": {
      "status": "healthy"
    }
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Mã trạng thái HTTP

- `200`: cơ sở dữ liệu và MinIO đều khả dụng.
- `503 DATABASE_UNAVAILABLE`: chỉ bước kiểm tra cơ sở dữ liệu Media thất bại.
- `503 STORAGE_UNAVAILABLE`: chỉ bước kiểm tra MinIO thất bại hoặc thiếu một vùng lưu trữ bắt buộc.
- `503 DEPENDENCY_UNAVAILABLE`: cả hai bước kiểm tra cơ sở dữ liệu và MinIO đều thất bại.

Tất cả phản hồi `503` sử dụng cấu trúc bao lỗi dùng chung:

```json
{
  "error": {
    "code": "STORAGE_UNAVAILABLE",
    "message": "Storage is temporarily unavailable.",
    "details": []
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

Điểm cuối chạy đồng thời các bước kiểm tra cơ sở dữ liệu và kho lưu trữ. Bước kiểm tra
cơ sở dữ liệu thực thi `SELECT 1`. Bước kiểm tra kho lưu trữ kiểm tra khả năng kết nối MinIO và
sự tồn tại của `images`, `videos`, `documents`, `audios` và `other` với khoảng thời gian chờ ngắn.
Cả hai bước kiểm tra đều không ghi dữ liệu hoặc tải đối tượng thử nghiệm lên.
