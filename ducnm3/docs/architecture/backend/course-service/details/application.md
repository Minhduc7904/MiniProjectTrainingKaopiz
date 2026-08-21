# Course Service — Application

## Mục đích

`CourseService.Application` là nơi đặt use case, validation và port mà API/Worker
gọi vào; nó chỉ được phép phụ thuộc Domain.

## Kiến trúc

```mermaid
flowchart LR
  API --> A[Application use case]
  A --> D[Domain]
  A --> P[Persistence/client port]
```

## Cách dùng

API nhận request, map thành command/query rồi inject handler/service Application.
Implementation của port được đăng ký tại Infrastructure composition root.

## Đã triển khai hiện tại

Course use case được đăng ký qua `AddCourseApplication()` và endpoint được map ở
`CourseService.Api/Program.cs`; Application không phụ thuộc ASP.NET Core hoặc EF Core.

`DeleteCourseHandler` snapshot usage ID theo scope Course/Lesson qua `ICourseMediaReader`, hard-delete aggregate tại Course repository, rồi gửi `DeleteMediaUsagesByIdsV1` cho Media Worker. Đây là luồng asynchronous không dùng distributed transaction.

## Định hướng/chưa triển khai

CRUD Course/Lesson, enrollment và progress chỉ được ghi là runtime khi handler
và endpoint tương ứng cùng hiện diện.

## Troubleshooting

| Hiện tượng | Cách xử lý |
| --- | --- |
| Handler cần DbContext | Đặt interface/repository port ở Application, không tham chiếu EF Core. |
