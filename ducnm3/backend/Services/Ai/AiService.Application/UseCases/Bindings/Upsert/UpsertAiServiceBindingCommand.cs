using AiService.Application.Repositories;

namespace AiService.Application.UseCases.Bindings.Upsert;

public sealed record UpsertAiServiceBindingCommand(string? ServiceName, Guid? ModelId);

public sealed record UpsertAiServiceBindingResult(AiServiceBindingRecord Binding, bool Created);
