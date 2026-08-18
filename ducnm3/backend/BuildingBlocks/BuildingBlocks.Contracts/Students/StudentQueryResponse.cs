namespace BuildingBlocks.Contracts.Students;

/// <summary>DTO dữ liệu học viên tối thiểu để service khác nhận qua HTTP query mà không tham chiếu domain của Student Service.</summary>
public sealed record StudentQueryResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Status);
