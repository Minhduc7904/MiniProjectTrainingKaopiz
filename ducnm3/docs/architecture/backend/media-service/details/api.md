# Media Service — API

## Mục đích

API là multipart/content HTTP boundary cho Media Service qua Gateway.

## Kiến trúc

```mermaid
flowchart LR
  Gateway --> Endpoints[Upload / Usage / Content / Thumbnail]
  Endpoints --> App[Application]
```

## Cách dùng

Endpoint parse request, map response DTO và gọi handler; không trả bucket/object key.

## Đã triển khai hiện tại

Có upload, usage, URL/content, thumbnail status/retry endpoint và Media health.

## Định hướng/chưa triển khai

Policy auth/authorization chỉ được coi là chạy khi endpoint source chứng minh.

## Troubleshooting

| Hiện tượng | Cách xử lý |
| --- | --- |
| Upload 413 | Kiểm tra payload limit và `ApiException` mapping. |
