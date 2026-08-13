# `GET /student/api/students`

Business flow:
[`get-students.md`](../../../business-flows/students/get-students.md).
Postman: `StudentService/GET Students`.

## Mục đích

Liệt kê Học viên theo offset pagination, hỗ trợ lọc theo trạng thái, sắp xếp và
nhảy trực tiếp tới một page. Student Service sở hữu endpoint và chỉ đọc
`lms_student_db`.

- Public Gateway path: `GET /student/api/students`.
- Direct service path: `GET /api/students`.
- Authentication/authorization: chưa triển khai trong phiên bản hiện tại.
- Cache: `Cache-Control: no-store`.

## Request

Endpoint không nhận request body.

Ví dụ:

```http
GET /student/api/students?status=ACTIVE&sortBy=createdAt&sortDirection=desc&page=1&pageSize=20
X-Correlation-ID: 6af909e3-3c95-4ab2-b7f2-c4dcac6351e4
```

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `status` | string | Không lọc | Không phân biệt hoa/thường; `ACTIVE`, `INACTIVE`, `BLOCKED`. Chuỗi rỗng được xem như không truyền. |
| `sortBy` | string | `createdAt` | Không phân biệt hoa/thường; `createdAt`, `displayName`, `email`. |
| `sortDirection` | string | `desc` | `asc` hoặc `desc`. |
| `page` | integer | `1` | Bắt đầu từ `1`; offset tính toán phải nằm trong giới hạn provider. |
| `pageSize` | integer | `20` | Từ `1` đến `100`. |

Các filter kết hợp bằng `AND`. Unknown query parameter không làm thay đổi
contract; chỉ các parameter trong allowlist trên được xử lý.

## Response thành công

`200 OK`

```json
{
  "data": [
    {
      "id": "22222222-2222-2222-2222-222222222222",
      "email": "student@example.com",
      "displayName": "Student One",
      "status": "ACTIVE",
      "createdAtUtc": "2026-08-13T03:00:00Z"
    }
  ],
  "meta": {
    "traceId": "0HNE7STUDENTLIST",
    "pagination": {
      "type": "offset",
      "page": 1,
      "pageSize": 20,
      "totalItems": 100000,
      "totalPages": 5000
    }
  }
}
```

`data` là array trực tiếp; không tạo thêm wrapper `items`. Khi không có kết quả,
endpoint vẫn trả `200` với `data: []`, `totalItems: 0` và `totalPages: 0`.

## Sắp xếp ổn định

Repository luôn thêm `id` làm unique tie-breaker và dùng cùng direction:

```text
createdAt desc -> created_at DESC, id DESC
displayName asc -> display_name ASC, id ASC
email asc -> email ASC, id ASC
```

`status + createdAt` sử dụng index hiện có
`ix_students_status_created_at`; `email` sử dụng unique index. Sort theo
`displayName` có thể cần filesort và phải được đánh giá lại nếu trở thành query
volume cao.

Offset pagination cung cấp totals và nhảy page nhưng row có thể dịch chuyển giữa
các page nếu có insert/delete đồng thời. Client cần refresh page khi yêu cầu
snapshot mới.

## Validation và lỗi

Query không hợp lệ trả `400 VALIDATION_FAILED`:

```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "field": "pageSize",
        "message": "Page size must be between 1 and 100."
      }
    ]
  },
  "meta": {
    "traceId": "0HNE7STUDENTLIST"
  }
}
```

Các field có thể xuất hiện trong `error.details`: `status`, `sortBy`,
`sortDirection`, `page`, `pageSize`.

## Side effects và test mapping

- GET safe, idempotent và chỉ đọc.
- Không thay đổi database, không phát message và không gọi service khác.
- Unit tests kiểm tra defaults, normalization, allowlist và page boundaries.
- Component tests kiểm tra binding, envelope, pagination metadata và lỗi `400`.
- Integration test dùng MySQL Testcontainer, migration thật và kiểm tra
  filter/order/page ổn định.
