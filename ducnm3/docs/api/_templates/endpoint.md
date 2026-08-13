# `METHOD /api/resource`

## Mục đích

Nêu kết quả đối với người dùng hoặc hệ thống và dịch vụ sở hữu.

## Xác thực và phân quyền

- Xác thực: bắt buộc hoặc không bắt buộc.
- Vai trò/phạm vi: các bên gọi được phép.
- Quy tắc sở hữu: cách dịch vụ xác nhận bên gọi được phép truy cập tài nguyên.

## Yêu cầu

### Tham số đường dẫn và truy vấn

Ghi rõ tên, kiểu, cờ bắt buộc, giá trị mặc định và quy tắc kiểm tra hợp lệ của từng tham số.

### Nội dung yêu cầu

```json
{
  "exampleField": "example value"
}
```

Ghi rõ kiểu, cờ bắt buộc, quy tắc kiểm tra hợp lệ và ý nghĩa nghiệp vụ của từng trường trong nội dung yêu cầu.

## Phản hồi thành công

Sử dụng cấu trúc bao trong [`../shared/response-format.md`](../shared/response-format.md).

```http
200 OK
```

```json
{
  "data": {
    "id": "example-id"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

Với điểm cuối dạng danh sách, sử dụng cấu trúc bao phân trang theo con trỏ hoặc độ lệch trong định dạng phản hồi dùng chung.

## Mã trạng thái HTTP

- `2xx`: hành vi khi thành công.
- `400`: yêu cầu không vượt qua kiểm tra hợp lệ.
- `401`: thiếu thông tin xác thực hoặc thông tin xác thực không hợp lệ.
- `403`: không đáp ứng yêu cầu phân quyền hoặc quyền sở hữu.
- `404`: không tìm thấy tài nguyên.
- `409`: xung đột nghiệp vụ hoặc xử lý đồng thời.
- `5xx`: lỗi không mong đợi từ dịch vụ hoặc thành phần phụ thuộc.

Chỉ liệt kê các mã mà điểm cuối thực sự có thể trả về và nêu mã lỗi cho từng lỗi dự kiến.

## Điều kiện nghiệp vụ và tác động phụ

- Điều kiện tiên quyết trước khi thực thi.
- Các bản ghi cơ sở dữ liệu được tạo hoặc thay đổi.
- Sự kiện, tác vụ nền, lời gọi HTTP bên ngoài hoặc thao tác MinIO.
- Hành vi lũy đẳng, phân trang, sắp xếp hoặc thử lại khi áp dụng.
