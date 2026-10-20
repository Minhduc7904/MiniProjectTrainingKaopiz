# Kiểm thử đơn vị AI Service

## Dự án

`backend/Services/Ai/AiService.UnitTests/AiService.UnitTests.csproj` (NUnit, kho giả trong bộ nhớ tại `Support/InMemoryAiRepositories.cs` và `FakeSecretProtector`; không dùng thư viện mock).

## Provider (`Application/Providers`)

| Kiểm thử | Mục đích / điều kiện đạt |
| --- | --- |
| `HandleAsyncStoresEncryptedKeyAndReturnsOnlyHint` | Tạo provider lưu ciphertext, kết quả chỉ có hint `****` + 4 ký tự cuối. |
| `HandleAsyncUsesExplicitBaseUrlForCustomProvider` | `CUSTOM` dùng base URL người dùng nhập. |
| `HandleAsyncRejectsInvalidInput` (8 case) | Tên, loại AI, base URL (thiếu với `CUSTOM`, sai scheme, có credentials), API key (thiếu, có khoảng trắng) trả `VALIDATION_FAILED` đúng field. |
| `HandleAsyncRejectsDuplicateNameCaseInsensitively` | Tên trùng không phân biệt hoa thường trả `AI_PROVIDER_NAME_CONFLICT`. |
| `UpdateKeepsStoredKeyWhenApiKeyIsBlank` / `UpdateReplacesStoredKeyWhenNewApiKeyIsProvided` | `apiKey` trống giữ key cũ; có giá trị thì thay và đổi hint. |
| `UpdateRejectsMissingProviderAndNameOfAnotherProvider`, `UpdateRequiresStatus` | 404 và 409 đúng mã; thiếu `status` bị từ chối. |
| `DeleteRemovesProviderWithoutModels`, `DeleteRejectsProviderThatStillHasModels`, `DeleteRejectsEmptyIdAndMissingProvider` | Xóa chỉ khi không còn model (`AI_PROVIDER_IN_USE`). |
| `GetAiProvidersQueryTests` (`CreateAppliesDefaultsAndNormalizesFilters`, `CreateRejectsInvalidValues`) | Mặc định page 1/size 20, filter chữ hoa, từ chối status/loại/page/pageSize sai. |

## Model (`Application/Models`)

`AiModelHandlerTests`: tạo mặc định `displayName`/`status`; từ chối provider thiếu, model trùng, tên sai; cập nhật giữ nguyên provider; từ chối đổi tên trùng model khác của provider; xóa model chưa gán và từ chối model đã gán (`AI_MODEL_IN_USE`).

## Binding (`Application/Bindings`)

`UpsertAiServiceBindingHandlerTests`: tạo rồi thay binding cho cùng service (`Created` đúng); từ chối `ai-service`, `lms-api-gateway`, service lạ, rỗng; từ chối `modelId` thiếu/rỗng; `AI_REFERENCE_INACTIVE` khi model hoặc provider `INACTIVE`; xóa binding và từ chối binding không tồn tại.

## Mã hóa (`Security/AesGcmSecretProtectorTests`)

Round-trip kể cả ký tự non-ASCII; nonce mới mỗi lần; ciphertext bị sửa hoặc key khác/payload hỏng làm `Unprotect` lỗi; `Mask` chỉ lộ 4 ký tự cuối; validator từ chối key thiếu hoặc sai kích thước.

Chạy:

```bash
dotnet test backend/Services/Ai/AiService.UnitTests/AiService.UnitTests.csproj
```
