# API của AI Service

AI Service lưu cấu hình AI dùng chung: **provider** (loại AI + API key mã hóa), **model** và **binding** (service nào dùng model nào). Toàn bộ endpoint chỉ dành cho actor `ADMIN`, đi qua Gateway với tiền tố `/ai`.

Phase 1 chỉ có cấu hình. Gọi LLM bằng OpenAI SDK và pipeline RabbitMQ thuộc Phase 2 và chưa được triển khai.

## Provider

- [`POST /ai/api/ai/providers` — Tạo AI provider](endpoints/post-providers.md)
- [`GET /ai/api/ai/providers` — Liệt kê AI provider](endpoints/get-providers.md)
- [`GET /ai/api/ai/providers/{providerId}` — Xem chi tiết AI provider](endpoints/get-provider-by-id.md)
- [`PUT /ai/api/ai/providers/{providerId}` — Cập nhật AI provider](endpoints/put-provider.md)
- [`DELETE /ai/api/ai/providers/{providerId}` — Xóa AI provider](endpoints/delete-provider.md)

## Model

- [`POST /ai/api/ai/models` — Tạo AI model](endpoints/post-models.md)
- [`GET /ai/api/ai/models` — Liệt kê AI model](endpoints/get-models.md)
- [`GET /ai/api/ai/models/{modelId}` — Xem chi tiết AI model](endpoints/get-model-by-id.md)
- [`PUT /ai/api/ai/models/{modelId}` — Cập nhật AI model](endpoints/put-model.md)
- [`DELETE /ai/api/ai/models/{modelId}` — Xóa AI model](endpoints/delete-model.md)

## Gán cho service

- [`GET /ai/api/ai/service-bindings` — Liệt kê AI đã gán cho service](endpoints/get-service-bindings.md)
- [`GET /ai/api/ai/service-bindings/{serviceName}` — Xem AI đã gán cho một service](endpoints/get-service-binding.md)
- [`PUT /ai/api/ai/service-bindings/{serviceName}` — Gán (hoặc đổi) model AI cho service](endpoints/put-service-binding.md)
- [`DELETE /ai/api/ai/service-bindings/{serviceName}` — Gỡ AI khỏi service](endpoints/delete-service-binding.md)

## Lựa chọn cho UI

- [`GET /ai/api/ai/options` — Lấy lựa chọn cố định cho UI cấu hình](endpoints/get-options.md)

## Ghi chú vận hành

- API key được mã hóa AES-256-GCM bằng master key `AI_ENCRYPTION_KEY` (env `Ai__Encryption__Key`). Mất key này thì không giải mã được key đã lưu và phải nhập lại. Xem [`docs/runbooks/ai-service.md`](../../runbooks/ai-service.md).
- Health: `GET /ai/health` kiểm tra database và messaging.
