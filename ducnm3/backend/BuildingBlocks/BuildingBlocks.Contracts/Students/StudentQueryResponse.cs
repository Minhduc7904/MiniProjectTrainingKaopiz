namespace BuildingBlocks.Contracts.Students;

public sealed record StudentQueryResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Status);
