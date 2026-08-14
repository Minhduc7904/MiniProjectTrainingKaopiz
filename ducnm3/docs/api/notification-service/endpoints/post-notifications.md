# `POST /api/notifications`

## Mục đích

Tạo ngay một thông báo `SINGLE`/`UNREAD` cho đúng một Học viên. Public route qua Gateway là `POST /notification/api/notifications`.

## Yêu cầu

MVP chưa có auth, vì vậy `createdBy` là UUID bắt buộc trong body.

```json
{
  "studentId": "11111111-1111-1111-1111-111111111111",
  "title": "Course update",
  "bodyMarkdown": "![Sơ đồ](/media/api/media/22222222-2222-2222-2222-222222222222/content)",
  "createdBy": "33333333-3333-3333-3333-333333333333"
}
```

- `studentId`, `createdBy`: UUID khác rỗng.
- `title`: bắt buộc, tối đa 200 ký tự.
- `bodyMarkdown`: bắt buộc. Markdown link tới media phải có đúng `contentUrl` dạng `/media/api/media/{mediaId}/content`.
- `![alt](contentUrl)` tạo usage `EMBED`; `[tệp](contentUrl)` tạo usage `ATTACHMENT`. Một cặp `(mediaId, usageType)` chỉ được giữ một lần theo thứ tự xuất hiện.

## Phản hồi thành công

```http
201 Created
Location: /notification/api/notifications/44444444-4444-4444-4444-444444444444
```

```json
{
  "data": {
    "id": "44444444-4444-4444-4444-444444444444",
    "recipientStudentId": "11111111-1111-1111-1111-111111111111",
    "title": "Course update",
    "bodyMarkdown": "![Sơ đồ](/media/api/media/22222222-2222-2222-2222-222222222222/content)",
    "sourceType": "SINGLE",
    "status": "UNREAD",
    "createdBy": "33333333-3333-3333-3333-333333333333",
    "createdAtUtc": "2026-08-14T01:00:00Z",
    "readAtUtc": null
  },
  "meta": { "traceId": "01J..." }
}
```

## Quy tắc và lỗi

- `400 VALIDATION_FAILED`: UUID, title, bodyMarkdown hoặc media `contentUrl` không hợp lệ.
- `503 SERVICE_UNAVAILABLE`: dependency hoặc hạ tầng tạm thời không sẵn sàng.
- Notification được ghi cùng transactional outbox. Chỉ khi tạo notification thành công, command `RegisterNotificationMediaUsageV1` mới được Media Worker tiêu thụ để tạo `media_usages` theo `owner_id = notification.id`.
- Nếu Media Worker xử lý thất bại, command được retry/redeliver; notification đã tạo không bị rollback.
