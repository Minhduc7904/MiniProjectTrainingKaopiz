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

## Chat completion (`Application/Completions`, `Infrastructure`)

`CreateAiCompletionHandlerTests` (kho giả + `FakeAiChatClient` trong `Support/`):

| Kiểm thử | Mục đích / điều kiện đạt |
| --- | --- |
| `BoundTaskCallsTheBoundModelWithTheDecryptedKeyAndLogsTheInvocation` | Gọi đúng model theo binding bằng key đã giải mã, trả text/tool call/token và ghi một dòng `ai_invocations` `SUCCESS`. |
| `TaskWithoutBindingIsAConflictAndDoesNotCallTheProvider` | `409 AI_TASK_NOT_CONFIGURED`, không gọi provider. |
| `InactiveModelIsRejected` | `409 AI_REFERENCE_INACTIVE`, không gọi provider. |
| `UnsupportedTaskFailsValidation` (2 case) | `task` rỗng hoặc lạ trả `VALIDATION_FAILED` trước khi tra binding. |
| `MalformedMessagesAndToolsAreListedTogether` | Role sai, message rỗng, tool trùng tên/thiếu schema… được liệt kê chung trong `error.details`. |
| `ProviderFailureIsMappedAndLoggedAsFailed` (6 case) | Mỗi loại lỗi provider ra đúng mã/HTTP status (`502`/`429`/`504`) và ghi `ai_invocations` `FAILED`. |
| `FailureToWriteTheLogDoesNotLoseTheModelResult` | Ghi nhật ký lỗi không làm mất kết quả model. |

`HttpAiChatClientTests` (`HttpMessageHandler` giả):

| Kiểm thử | Mục đích / điều kiện đạt |
| --- | --- |
| `SendsBearerKeyToChatCompletionsWithModelMessagesAndTools` | `POST {baseUrl}/chat/completions` với `Authorization: Bearer`, model, messages, tools. |
| `OmitsToolsPropertyWhenThereAreNoTools` | Không gửi `tools` rỗng. |
| `AnthropicAlsoSendsNativeHeaders` | Thêm `x-api-key` và `anthropic-version`. |
| `ImageFilePartsAndToolMessagesAreSerializedInOpenAiShape` | Ảnh qua `image_url`, PDF qua phần `file`, message `tool` có `tool_call_id`. |
| `ToolCallsAreParsedAndEmptyIdsAreReplaced` | Parse `tool_calls`; id rỗng được thay bằng id sinh sẵn. |
| `HttpFailureIsClassified` | 401/403, 429, 5xx, 400… phân loại đúng `AiChatFailureKind`. |
| `RejectedRequestKeepsAShortProviderMessageButMasksTheKey` | Giữ phần rút gọn lỗi provider nhưng che API key. |
| `UnreadableSuccessBodyIsInvalidResponse` | Body không đọc được → `InvalidResponse`. |
| `NetworkFailureIsUnreachableAndDoesNotLeakTheException` | Lỗi mạng → `Unreachable`, không lộ message exception. |
| `InvalidBaseUrlFailsWithoutCallingOut` | Base URL không hợp lệ không gọi ra ngoài. |
| `CallerCancellationPropagates` | Hủy từ caller được truyền lên, không đổi thành timeout. |

## Thử kết nối (`Application/Providers`, `Infrastructure`)

`CheckAiProviderHealthHandlerTests`: provider bật được giải mã key rồi gọi probe; provider `INACTIVE` trả `DISABLED` và không gọi ra ngoài; kết quả lỗi của probe được giữ nguyên; key không giải mã được trả `ERROR` và không gọi ra ngoài; provider không tồn tại `404`, ID rỗng `400`; thử tất cả vẫn liệt kê provider tắt.

`HttpAiProviderProbeTests`: gọi `{baseUrl}/models` với `Authorization: Bearer`; `ANTHROPIC` gửi thêm `x-api-key` và `anthropic-version`; 401/403 → `AUTH_FAILED`, 404/500 → `ERROR`; lỗi mạng → `UNREACHABLE` và không lộ message của exception; base URL không phải http(s) → `ERROR` không gọi ra ngoài; hủy từ phía caller được truyền lên.

## Mã hóa (`Security/AesGcmSecretProtectorTests`)

Round-trip kể cả ký tự non-ASCII; nonce mới mỗi lần; ciphertext bị sửa hoặc key khác/payload hỏng làm `Unprotect` lỗi; `Mask` chỉ lộ 4 ký tự cuối; validator từ chối key thiếu hoặc sai kích thước.

Chạy:

```bash
dotnet test backend/Services/Ai/AiService.UnitTests/AiService.UnitTests.csproj
```
