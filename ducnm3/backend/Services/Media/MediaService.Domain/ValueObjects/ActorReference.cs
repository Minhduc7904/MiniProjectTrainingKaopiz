// File: backend/Services/Media/MediaService.Domain/ValueObjects/ActorReference.cs
// Mục đích: Đóng gói định danh và loại actor để truyền thông tin người thực hiện qua use case một cách nhất quán.

namespace MediaService.Domain.ValueObjects;

public sealed record ActorReference(string Type, Guid Id)
{
    public ActorReference Normalize() =>
        this with { Type = Type.Trim().ToUpperInvariant() };
}
