# Gửi một thông báo

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`POST /api/notifications`

Yêu cầu:

```json
{"studentId":"student-uuid","title":"Course update","bodyMarkdown":"New material is available."}
```

Dữ liệu phản hồi thành công `201 Created`:

```json
{"id":"notification-uuid","recipientStudentId":"student-uuid","status":"UNREAD","sourceType":"SINGLE"}
```

- Kiểm tra hợp lệ: các ID là UUID; `title` là bắt buộc, dài 1–200 ký tự; Markdown được làm sạch và các URL phương tiện thuộc sở hữu của Media Service.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 STUDENT_NOT_FOUND`.
- Tác động phụ: tạo một bản ghi `notifications` với `source_type = SINGLE`.
