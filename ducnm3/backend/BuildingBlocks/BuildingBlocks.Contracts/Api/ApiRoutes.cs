using System.Globalization;

namespace BuildingBlocks.Contracts.Api;

public static class ApiRoutes
{
    public static class Media
    {
        public const string Upload = "/api/media";
        public const string Usages = "/api/media/usages";
        public const string UsageUrlTemplate = "/api/media/usages/{usageId}/url";
        public const string UsageUrls = "/api/media/usages/urls";
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

        public static string UsageUrlServicePath(Guid usageId) =>
            BuildServicePath(FormatGuidRoute(UsageUrlTemplate, "usageId", usageId));

        public static string UsageUrlPublicPath(Guid usageId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(UsageUrlTemplate, "usageId", usageId));

        public static string UsageUrlsServicePath() =>
            BuildServicePath(UsageUrls);

        public static string UsageUrlsPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Media, UsageUrls);

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

    public static class Notifications
    {
        public const string Items = "/api/notifications";
        public const string ItemByIdTemplate = "/api/notifications/{notificationId}";
        public const string Batches = "/api/notification-batches";
        public const string BatchByIdTemplate = "/api/notification-batches/{batchId}";
        public const string BatchFailedItemsTemplate = "/api/notification-batches/{batchId}/failed-items";

        public static string BatchByIdServicePath(Guid batchId) =>
            BuildServicePath(FormatGuidRoute(BatchByIdTemplate, "batchId", batchId));

        public static string BatchByIdPublicPath(Guid batchId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Notification,
                FormatGuidRoute(BatchByIdTemplate, "batchId", batchId));

        public static string BatchFailedItemsServicePath(Guid batchId) =>
            BuildServicePath(
                FormatGuidRoute(BatchFailedItemsTemplate, "batchId", batchId));

        public static string ItemByIdServicePath(Guid notificationId) =>
            BuildServicePath(FormatGuidRoute(ItemByIdTemplate, "notificationId", notificationId));

        public static string ItemByIdPublicPath(Guid notificationId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Notification,
                FormatGuidRoute(ItemByIdTemplate, "notificationId", notificationId));
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
