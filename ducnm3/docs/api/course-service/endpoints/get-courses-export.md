# `GET /course/api/courses/export`

Business flow: [Xuất CSV Course](../../../business-flows/courses/get-courses-export.md).

## Mục đích

Course Service xuất danh sách Course lớn dưới dạng CSV download. Direct path là
`GET /api/courses/export`; endpoint chỉ đọc, safe và idempotent.

## Xác thực và phân quyền

- Xác thực và phân quyền: theo policy Gateway/Course Service hiện hành.
- Visibility: Course Service chỉ đọc dữ liệu Course do service sở hữu.

## Yêu cầu

Không có request body.

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `status` | string | Không có | Optional; trim/uppercase và chỉ nhận `DRAFT`, `PUBLISHED`, `ARCHIVED`. |

Ví dụ: `GET /course/api/courses/export?status=PUBLISHED`.

## Phản hồi thành công

`200 OK` với `Content-Type: text/csv; charset=utf-8`,
`Content-Disposition: attachment; filename="courses.csv"` và
`Cache-Control: no-store`.

Response là ngoại lệ binary/stream của JSON envelope chuẩn: bắt đầu bằng UTF-8
BOM để Microsoft Excel hiển thị tiếng Việt đúng, sau đó là:

```csv
id,name,status,createdAtUtc
course-uuid,Backend Fundamentals,PUBLISHED,2026-08-19T01:00:00.0000000Z
```

Mỗi field chứa comma, quote, CR hoặc LF được bọc bằng `"`; quote bên trong được
đổi thành `""` theo RFC 4180.

## Streaming và tính nhất quán

Infrastructure đọc projection tối đa 500 rows/chunk với `AsNoTracking()` theo
keyset `created_at DESC, id DESC`; filter `status` được áp dụng trước keyset.
API ghi BOM, header và từng row trực tiếp vào response body; không materialize
toàn bộ dataset hay CSV string. `RequestAborted` được truyền đến query và write
để client cancel dừng export.

Export là nhiều read query nên không phải transaction snapshot: Course được
thêm/xóa/sửa trong lúc export có thể không xuất hiện hoặc thay đổi vị trí. Keyset
ngăn Course đã đọc bị lặp lại trong cùng stream.

## Mã trạng thái HTTP

- `200`: CSV stream được khởi tạo thành công, kể cả khi chỉ có header.
- `400 VALIDATION_FAILED`: `status` không hợp lệ; trả JSON error envelope trước
  khi bắt đầu CSV stream.

## Điều kiện nghiệp vụ và tác động phụ

Không thay đổi bảng, không phát message và không gọi service ngoài. Postman item:
`CourseService/GET Export courses CSV`.
