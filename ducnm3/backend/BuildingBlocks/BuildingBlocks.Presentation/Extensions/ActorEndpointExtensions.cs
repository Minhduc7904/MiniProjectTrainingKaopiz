using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;

namespace BuildingBlocks.Presentation.Extensions;

/// <summary>Áp policy actor header thống nhất cho Minimal API endpoint cần audit hoặc phân quyền.</summary>
public static class ActorEndpointExtensions
{
    private const string ActorContextItemKey = "BuildingBlocks.Presentation.ActorContext";

    public static RouteHandlerBuilder RequireActor(
        this RouteHandlerBuilder endpoint,
        ActorAccess access)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (access == ActorAccess.None)
        {
            throw new ArgumentOutOfRangeException(nameof(access));
        }

        return endpoint.AddEndpointFilter(async (invocationContext, next) =>
        {
            var actor = ReadActor(invocationContext.HttpContext.Request);
            if (!IsAllowed(actor.Type, access))
            {
                throw new ActorHeaderException(
                    ApiErrorCodes.Forbidden,
                    "The actor is not allowed to access this endpoint.",
                    StatusCodes.Status403Forbidden);
            }

            invocationContext.HttpContext.Items[ActorContextItemKey] = actor;
            return await next(invocationContext);
        });
    }

    public static ActorContext GetRequiredActor(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.TryGetValue(ActorContextItemKey, out var value) && value is ActorContext actor
            ? actor
            : throw new InvalidOperationException("Actor policy is required before reading the actor.");
    }

    private static ActorContext ReadActor(HttpRequest request)
    {
        var type = request.Headers[ApiHeaderNames.ActorType].ToString().Trim().ToUpperInvariant();
        var idText = request.Headers[ApiHeaderNames.ActorId].ToString();
        if (!IsKnownType(type) || !Guid.TryParse(idText, out var id) || id == Guid.Empty)
        {
            throw new ActorHeaderException(
                ApiErrorCodes.ValidationFailed,
                $"{ApiHeaderNames.ActorType} must be ADMIN or STUDENT and {ApiHeaderNames.ActorId} must be a valid UUID.",
                StatusCodes.Status400BadRequest);
        }

        return new ActorContext(type, id);
    }

    private static bool IsKnownType(string type) => type is ActorHeaderTypes.Admin or ActorHeaderTypes.Student;

    private static bool IsAllowed(string type, ActorAccess access) =>
        (type == ActorHeaderTypes.Admin && access.HasFlag(ActorAccess.Admin)) ||
        (type == ActorHeaderTypes.Student && access.HasFlag(ActorAccess.Student));
}
