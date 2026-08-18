// File: backend/Services/Media/MediaService.Domain/ValueObjects/ActorReference.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Domain.ValueObjects;

public sealed record ActorReference(string Type, Guid Id)
{
    public ActorReference Normalize() =>
        this with { Type = Type.Trim().ToUpperInvariant() };
}
