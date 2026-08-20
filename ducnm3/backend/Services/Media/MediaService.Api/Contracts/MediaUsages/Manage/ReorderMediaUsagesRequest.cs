namespace MediaService.Api.Contracts.Requests;

public sealed record ReorderMediaUsagesRequest(
    string OwnerService,
    string OwnerType,
    string OwnerId,
    IReadOnlyList<string> UsageIds);
