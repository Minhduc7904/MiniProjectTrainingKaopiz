using System.Globalization;

namespace BuildingBlocks.Contracts.Api;

public static class ApiRoutes
{
    public static class Media
    {
        public const string Upload = "/api/media";
        public const string Usages = "/api/media/usages";
        public const string ContentTemplate = "/api/media/{mediaId}/content";
        public const string ThumbnailStatusTemplate =
            "/api/media/{mediaId}/thumbnail";
        public const string ThumbnailRetryTemplate =
            "/api/media/{mediaId}/thumbnail/retry";

        public static string ContentServicePath(Guid mediaId) =>
            BuildServicePath(FormatGuidRoute(ContentTemplate, "mediaId", mediaId));

        public static string ContentPublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(ContentTemplate, "mediaId", mediaId));

        public static string ThumbnailStatusServicePath(Guid mediaId) =>
            BuildServicePath(
                FormatGuidRoute(ThumbnailStatusTemplate, "mediaId", mediaId));

        public static string ThumbnailStatusPublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(ThumbnailStatusTemplate, "mediaId", mediaId));

        public static string ThumbnailRetryServicePath(Guid mediaId) =>
            BuildServicePath(
                FormatGuidRoute(ThumbnailRetryTemplate, "mediaId", mediaId));

        public static string ThumbnailRetryPublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(ThumbnailRetryTemplate, "mediaId", mediaId));
    }

    public static class Students
    {
        public const string List = "/api/students";
        public const string GetByIdTemplate = "/api/students/{studentId}";

        public static string ListServicePath() =>
            BuildServicePath(List);

        public static string ListPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Student, List);

        public static string GetByIdServicePath(Guid studentId) =>
            BuildServicePath(
                FormatGuidRoute(GetByIdTemplate, "studentId", studentId));

        public static string GetByIdPublicPath(Guid studentId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Student,
                FormatGuidRoute(GetByIdTemplate, "studentId", studentId));
    }

    public static string BuildServicePath(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        return route.TrimStart('/');
    }

    public static string BuildPublicPath(string gatewayRoutePrefix, string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gatewayRoutePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        return string.Concat(
            "/",
            gatewayRoutePrefix.Trim('/'),
            "/",
            route.TrimStart('/'));
    }

    private static string FormatGuidRoute(
        string template,
        string parameterName,
        Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                $"{parameterName} must not be empty.",
                parameterName);
        }

        return template.Replace(
            $"{{{parameterName}}}",
            value.ToString("D", CultureInfo.InvariantCulture),
            StringComparison.Ordinal);
    }
}
