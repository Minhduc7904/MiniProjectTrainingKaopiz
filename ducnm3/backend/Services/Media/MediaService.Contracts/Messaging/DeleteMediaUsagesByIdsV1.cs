using BuildingBlocks.Messaging.Abstractions;

namespace MediaService.Contracts.Messaging;

/// <summary>Dọn các media usage đã được owner service xóa aggregate.</summary>
public sealed record DeleteMediaUsagesByIdsV1(IReadOnlyList<Guid> UsageIds) : ICommand;
