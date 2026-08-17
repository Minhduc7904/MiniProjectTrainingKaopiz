# Clean Architecture và BuildingBlocks

~~~text
API / Worker → Application → Domain
Infrastructure → Application
Domain → không phụ thuộc layer nào
~~~

- **Domain**: entity, enum, value object và business rule; không biết HTTP, EF Core, RabbitMQ hoặc MinIO.
- **Application**: use case, validation và port/abstraction; chỉ phụ thuộc Domain.
- **Infrastructure**: EF Core/MySQL, typed HTTP client, MassTransit và MinIO adapter.
- **API/Worker**: transport, middleware và DI composition root; không đặt business rule.

BuildingBlocks cung cấp response envelope, middleware, messaging, HTTP client và migration runner dùng chung. Xem chi tiết dependency và từng nhóm tại [kiến trúc BuildingBlocks](../building-blocks/architecture.md). Unit test nằm cạnh service/layer; cross-service/E2E nằm ở tests.
