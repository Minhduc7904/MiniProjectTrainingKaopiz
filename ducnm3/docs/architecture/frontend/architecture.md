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

## Đã triển khai hiện tại

Vite + React, Tailwind, React Router, Redux Toolkit, Axios, toast interceptor, trang Student, Media và Notification Batch. Xem [Backend overview](../backend/overview.md).

## Định hướng/chưa triển khai

Trang mới tuân theo Page → Hook → Redux → API; không thêm Axios call trực tiếp trong UI component.

