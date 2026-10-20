# Kiểm thử component AI Service

## Dự án

`backend/Services/Ai/AiService.ComponentTests/AiService.ComponentTests.csproj`. Dùng `TestServer` với kho giả trong bộ nhớ và `FakeSecretProtector`, đi qua pipeline HTTP thật (middleware, actor, envelope).

`Endpoints/AiConfigurationEndpointsComponentTests.cs`:

| Kiểm thử | Mục đích / điều kiện đạt |
| --- | --- |
| `CreateProviderReturnsCreatedWithHintAndNeverEchoesTheApiKey` | `201`, header `Location`; response và JSON không chứa API key hay ciphertext. |
| `ProviderEndpointsRequireAnAdminActor` | Thiếu header actor trả `400`; actor Student trả `403 FORBIDDEN`. |
| `CreateProviderWithInvalidBodyReturnsValidationDetailsAndPersistsNothing` | `400` kèm `error.details`, không ghi dữ liệu. |
| `CreateProviderWithDuplicateNameReturnsConflict` | `409 AI_PROVIDER_NAME_CONFLICT`. |
| `GetProvidersReturnsOffsetPaginationAndNoSecrets` | `meta.pagination` offset, không lộ secret. |
| `GetProviderWithMalformedOrUnknownIdReturnsValidationOrNotFound` | `400` khi ID hỏng, `404` khi không tồn tại. |
| `UpdateProviderWithoutApiKeyKeepsStoredKey` | PUT không có `apiKey` giữ nguyên key. |
| `DeleteProviderWithModelsReturnsConflictThenNoContentAfterModelRemoved` | `409` rồi `204` sau khi xóa model. |
| `ModelLifecycleCreateListUpdateAndDetail` | Vòng đời model. |
| `CreateModelForUnknownProviderReturnsNotFound` | `404 AI_PROVIDER_NOT_FOUND`. |
| `BindingLifecycleUpsertCreatedThenReplacedThenBlockedModelDeleteAndRemoval` | PUT `201` rồi `200`; xóa model đang gán `409`; xóa binding `204`. |
| `UpsertBindingRejectsUnsupportedServiceAndInactiveModel` | `400` service lạ; `409 AI_REFERENCE_INACTIVE`. |
| `GetOptionsListsProviderTypesWithDefaultBaseUrlsAndBindableServices` | Trả loại AI kèm base URL mặc định và service được phép gán. |

Chạy:

```bash
dotnet test backend/Services/Ai/AiService.ComponentTests/AiService.ComponentTests.csproj
```

## Chưa có

Chưa có kiểm thử tích hợp (Testcontainers MySQL) cho migration và repository EF; migration đã được xác nhận thủ công qua Docker end-to-end.
