# `GET /media/api/media/{mediaId}/thumbnail`

Trả trạng thái tạo thumbnail của media gốc. Endpoint trả `200` cùng
`QUEUED`, `PROCESSING`, `READY` hoặc `FAILED`; response dùng
`Cache-Control: no-store` vì trạng thái thay đổi bất đồng bộ.

```json
{
  "data": {
    "sourceMediaId": "8c2bf508-60bb-44d4-91aa-1baad98db09c",
    "jobId": "98431cd6-9bab-480f-bbb8-659a36a6be8e",
    "status": "READY",
    "thumbnailMediaId": "7b920767-6924-42b1-889f-5e59d3e8f69f",
    "activeThumbnailMediaId": "7b920767-6924-42b1-889f-5e59d3e8f69f",
    "thumbnailContentUrl": "/media/api/media/7b920767-6924-42b1-889f-5e59d3e8f69f/content",
    "lastError": null,
    "updatedAtUtc": "2026-08-13T10:00:00Z"
  }
}
```

`404 MEDIA_THUMBNAIL_NOT_FOUND` áp dụng khi media không tồn tại, không hỗ trợ
thumbnail hoặc chưa có derivation job. `lastError` chỉ chứa thông báo an toàn,
không chứa stderr, object key hay credential.
