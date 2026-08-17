# Scheduler Service — Testing

## Mục đích

Unit test hiện bảo vệ health adapter Scheduler.

## Kiến trúc

```mermaid
flowchart LR
  UnitTests --> Health[SchedulerDatabaseHealthProbe]
```

## Cách dùng

```bash
dotnet test backend/Services/Scheduler/SchedulerService.UnitTests
```

## Đã triển khai hiện tại

Có `SchedulerDatabaseHealthProbeTests`; chưa có integration/component test project.

## Định hướng/chưa triển khai

Thêm test Cron/claim/handler đồng thời với runtime feature tương ứng.

## Troubleshooting

| Hiện tượng | Cách xử lý |
| --- | --- |
| Test thiếu DB | Health unit test nên fake/probe dependency, không cần DB thật. |
