namespace MediaService.Domain.Actors;

public sealed record ActorReference(string Type, Guid Id)
{
    public ActorReference Normalize() =>
        this with { Type = Type.Trim().ToUpperInvariant() };
}
