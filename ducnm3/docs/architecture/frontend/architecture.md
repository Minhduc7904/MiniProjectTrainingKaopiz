# Kiến trúc Frontend

Frontend là React SPA gọi Backend qua YARP Gateway; không gọi microservice trực tiếp.

~~~mermaid
flowchart LR
  Page --> Hook --> Redux[Redux Toolkit]
  Hook --> Api[Axios client]
  Api --> Gateway[YARP Gateway]
  Redux --> Components
~~~

## Các tầng

- **Page:** composition Workbench Input/Output, không gọi Axios.
- **Hook:** entry point load/mutate của Page.
- **Redux feature:** data, pagination, loading, success, error và trace ID.
- **API:** Axios client, route, envelope mapping và interceptor.
- **UI/layout:** chỉ nhận props, không biết Redux/API.

Với result Media, `useMediaThumbnail` sở hữu vòng đời polling ngắn hạn và hủy
timer/request khi media đổi, Reset hoặc unmount. API layer đọc `thumbnailStatusUrl`
do backend trả về và gọi retry bằng `mediaId`; `MediaImagePreview` lấy binary
qua `GET contentUrl` của `httpClient`, tạo object URL tạm thời và thu hồi khi
unmount. Component chỉ nhận state để hiển thị preview, metadata và thumbnail
status.

## Đã triển khai hiện tại

Vite + React, Tailwind, React Router, Redux Toolkit, Axios, toast interceptor, trang Student, Media và Notification Batch. Notification có ba menu tách biệt: tạo batch, quản lý list offset có action retry failed và tiến trình theo URL chứa `batchId`. Trang tiến trình polling tuần tự snapshot status, delivery status rồi Media Usage job status; bước sau không request trước khi bước trước terminal. Hai trang Media dùng chung preview ảnh và polling/retry thumbnail; trang direct upload hiển thị progress XHR ở Output panel. Xem [Backend overview](../backend/overview.md).

## Định hướng/chưa triển khai

Trang mới tuân theo Page → Hook → Redux → API; không thêm Axios call trực tiếp trong UI component.
