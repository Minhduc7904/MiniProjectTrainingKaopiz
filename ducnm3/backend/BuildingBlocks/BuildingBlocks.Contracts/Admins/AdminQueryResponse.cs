namespace BuildingBlocks.Contracts.Admins;

public sealed record AdminQueryResponse(
    Guid Id,
    string DisplayName,
    string Status);