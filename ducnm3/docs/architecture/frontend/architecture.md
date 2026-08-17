# Kiến trúc Frontend

Frontend là React SPA gọi Backend qua YARP Gateway; không gọi microservice trực tiếp.

~~~mermaid
flowchart LR
  Page --> Hook --> Redux[Redux Toolkit]
  Hook --> Api[Axios client]
  Api --> Gateway[YARP Gateway]
  Redux --> Components
~~~

## Cấu trúc thư mục

```text
frontend/
└── lms-web/
    ├── public/
    ├── src/
    │   ├── api/                    # Axios client và API adapter
    │   ├── app/                    # Store/router/bootstrap
    │   ├── assets/
    │   ├── components/
    │   │   ├── layout/
    │   │   └── ui/
    │   ├── constants/
    │   │   ├── activities/
    │   │   └── inputs/
    │   ├── features/
    │   │   ├── media/
    │   │   ├── notifications/
    │   │   ├── students/
    │   │   └── toasts/
    │   ├── hooks/
    │   │   ├── media/
    │   │   ├── notifications/
    │   │   ├── students/
    │   │   ├── toasts/
    │   │   └── ui/
    │   ├── pages/
    │   │   ├── media/
    │   │   ├── notifications/
    │   │   ├── placeholder/
    │   │   └── students/
    │   └── theme/
    ├── index.html
    ├── package.json
    └── vite.config.js
```

Page composition đi qua Hook/Redux/API theo cùng domain. `components/ui` không
được import Axios hoặc biết response envelope của backend.

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
