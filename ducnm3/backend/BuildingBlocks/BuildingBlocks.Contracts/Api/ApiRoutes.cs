using System.Globalization;

namespace BuildingBlocks.Contracts.Api;

public static class ApiRoutes
{
    public static class Media
    {
        public const string Upload = "/api/media";
        public const string Usages = "/api/media/usages";
        public const string ContentTemplate = "/api/media/{mediaId}/content";

        public static string ContentServicePath(Guid mediaId) =>
            BuildServicePath(FormatGuidRoute(ContentTemplate, "mediaId", mediaId));

        public static string ContentPublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(ContentTemplate, "mediaId", mediaId));
    }

    public static class Students
    {
        public const string GetByIdTemplate = "/api/students/{studentId}";

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
