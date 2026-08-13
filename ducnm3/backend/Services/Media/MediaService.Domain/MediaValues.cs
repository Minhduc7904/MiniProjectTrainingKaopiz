namespace MediaService.Domain;

public static class ActorTypes
{
    public const string Student = "STUDENT";
}

public static class MediaTypes
{
    public const string Image = "IMAGE";
    public const string Video = "VIDEO";
    public const string Document = "DOCUMENT";
    public const string Audio = "AUDIO";
    public const string Other = "OTHER";
}

public static class MediaObjectStatuses
{
    public const string Pending = "PENDING";
    public const string Ready = "READY";
    public const string Failed = "FAILED";
}

public static class MediaOwnerServices
{
    public const string Student = "STUDENT";
}

public static class MediaOwnerTypes
{
    public const string StudentAvatar = "STUDENT_AVATAR";
}

public static class MediaUsageTypes
{
    public const string Avatar = "AVATAR";
}

public sealed record ActorReference(string Type, Guid Id)
{
    public ActorReference Normalize() =>
        this with { Type = Type.Trim().ToUpperInvariant() };
}
