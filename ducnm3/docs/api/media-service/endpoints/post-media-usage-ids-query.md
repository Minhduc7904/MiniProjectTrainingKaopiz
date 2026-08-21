# Truy vấn media usage ID theo owner

Business flow: [`post-media-usage-ids-query.md`](../../../business-flows/media/post-media-usage-ids-query.md).

`POST /api/media/usages/ids:query` là query nội bộ giữa service, trả các `mediaUsageId` còn hoạt động theo một hay nhiều owner scope. Endpoint không tạo/sửa dữ liệu.

## Request

```json
{
  "owners": [
    {
      "ownerService": "COURSE",
      "ownerType": "COURSE_DESCRIPTION",
      "ownerId": "11111111-1111-1111-1111-111111111111"
    }
  ]
}
```

Mỗi owner phải có service/type không rỗng và UUID `ownerId` hợp lệ.

## Response

```json
{
  "data": ["22222222-2222-2222-2222-222222222222"],
  "meta": { "traceId": "01J..." }
}
```

- `200`: query hợp lệ; mảng rỗng nghĩa là không còn usage active.
- `400 INVALID_MEDIA`: owner scope không hợp lệ.
