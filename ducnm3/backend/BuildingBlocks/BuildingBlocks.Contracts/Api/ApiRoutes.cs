using System.Globalization;

namespace BuildingBlocks.Contracts.Api;

/// <summary>
/// Nguồn duy nhất cho route contract giữa service, Gateway và client nội bộ.
/// Dùng các hàm <c>*ServicePath</c> cho YARP/downstream và <c>*PublicPath</c> cho URL đi qua Gateway.
/// </summary>
public static class ApiRoutes
{
    public static class Admins
    {
        public const string GetByIdTemplate = "/api/admins/{adminId}";

        public static string GetByIdServicePath(Guid adminId) =>
            BuildServicePath(FormatGuidRoute(GetByIdTemplate, "adminId", adminId));

        public static string GetByIdPublicPath(Guid adminId) =>
            BuildPublicPath(GatewayRoutePrefixes.Admin, FormatGuidRoute(GetByIdTemplate, "adminId", adminId));
    }

    /// <summary>Route contract do Course Service sở hữu.</summary>
    public static class Courses
    {
        public const string List = "/api/courses";
        public const string Export = "/api/courses/export";
        public const string BufferedExportBenchmark = "/api/performance/courses/export-buffered";
        public const string DetailsTemplate = "/api/courses/{courseId}/details";
        public const string LessonsTemplate = "/api/courses/{courseId}/lessons";
        public const string ByIdTemplate = "/api/courses/{courseId}";
        public const string LessonByIdTemplate = "/api/courses/{courseId}/lessons/{lessonId}";
        public const string LessonReorderTemplate = "/api/courses/{courseId}/lessons/reorder";

        public static string ListServicePath() =>
            BuildServicePath(List);

        public static string ListPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Course, List);

        public static string ByIdServicePath(Guid courseId) =>
            BuildServicePath(FormatGuidRoute(ByIdTemplate, "courseId", courseId));

        public static string DetailsPublicPath(Guid courseId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Course,
                FormatGuidRoute(DetailsTemplate, "courseId", courseId));

        public static string LessonByIdServicePath(Guid courseId, Guid lessonId) =>
            BuildServicePath(FormatGuidRoute(FormatGuidRoute(LessonByIdTemplate, "courseId", courseId), "lessonId", lessonId));

        public static string LessonReorderServicePath(Guid courseId) =>
            BuildServicePath(FormatGuidRoute(LessonReorderTemplate, "courseId", courseId));

        public static string ExportServicePath() =>
            BuildServicePath(Export);

        public static string ExportPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Course, Export);

        public static string BufferedExportBenchmarkServicePath() =>
            BuildServicePath(BufferedExportBenchmark);

        public static string BufferedExportBenchmarkPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Course, BufferedExportBenchmark);
    }

    /// <summary>Route contract do Media Service sở hữu.</summary>
    public static class Media
    {
        public const string Upload = "/api/media";
        public const string Library = "/api/media/library";
        public const string UploadIntents = "/api/media/upload-intents";
        public const string UploadCompleteTemplate = "/api/media/{mediaId}/upload-complete";
        public const string Usages = "/api/media/usages";
        public const string UsageBatch = "/api/media/usages/batch";
        public const string UsageReorder = "/api/media/usages/reorder";
        public const string UsageUrlTemplate = "/api/media/usages/{usageId}/url";
        public const string UsageUrls = "/api/media/usages/urls";
        public const string ContentTemplate = "/api/media/{mediaId}/content";
        public const string ThumbnailStatusTemplate =
            "/api/media/{mediaId}/thumbnail";
        public const string ThumbnailRetryTemplate =
            "/api/media/{mediaId}/thumbnail/retry";
        public const string NotificationMediaUsageJobStatusTemplate =
            "/api/media/usage-jobs/{jobId}/status";

        public static string ResourcePublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                $"{Upload}/{mediaId:D}");

        public static string UploadCompleteServicePath(Guid mediaId) =>
            BuildServicePath(
                FormatGuidRoute(UploadCompleteTemplate, "mediaId", mediaId));

        /// <summary>Nhận media ID hợp lệ và trả path content không có dấu <c>/</c> đầu để dùng làm destination service.</summary>
        public static string ContentServicePath(Guid mediaId) =>
            BuildServicePath(FormatGuidRoute(ContentTemplate, "mediaId", mediaId));

        /// <summary>Nhận media ID hợp lệ và trả URL path public đã ghép prefix Gateway.</summary>
        public static string ContentPublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(ContentTemplate, "mediaId", mediaId));

        /// <summary>Trả path nội bộ để lấy signed URL của một media usage.</summary>
        public static string UsageUrlServicePath(Guid usageId) =>
            BuildServicePath(FormatGuidRoute(UsageUrlTemplate, "usageId", usageId));

        public static string UsageDeleteTemplatePath(Guid usageId) =>
            BuildServicePath($"{Usages}/{usageId:D}");

        /// <summary>Trả path Gateway để client lấy signed URL của một media usage.</summary>
        public static string UsageUrlPublicPath(Guid usageId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(UsageUrlTemplate, "usageId", usageId));

        /// <summary>Trả path nội bộ cho truy vấn signed URL theo lô.</summary>
        public static string UsageUrlsServicePath() =>
            BuildServicePath(UsageUrls);

        /// <summary>Trả path public cho truy vấn signed URL theo lô.</summary>
        public static string UsageUrlsPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Media, UsageUrls);

        /// <summary>Trả path nội bộ để xem trạng thái thumbnail của media.</summary>
        public static string ThumbnailStatusServicePath(Guid mediaId) =>
            BuildServicePath(
                FormatGuidRoute(ThumbnailStatusTemplate, "mediaId", mediaId));

        /// <summary>Trả path Gateway để xem trạng thái thumbnail của media.</summary>
        public static string ThumbnailStatusPublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(ThumbnailStatusTemplate, "mediaId", mediaId));

        /// <summary>Trả path nội bộ để yêu cầu retry xử lý thumbnail.</summary>
        public static string ThumbnailRetryServicePath(Guid mediaId) =>
            BuildServicePath(
                FormatGuidRoute(ThumbnailRetryTemplate, "mediaId", mediaId));

        /// <summary>Trả path Gateway để yêu cầu retry xử lý thumbnail.</summary>
        public static string ThumbnailRetryPublicPath(Guid mediaId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(ThumbnailRetryTemplate, "mediaId", mediaId));

        public static string NotificationMediaUsageJobStatusServicePath(Guid jobId) =>
            BuildServicePath(
                FormatGuidRoute(NotificationMediaUsageJobStatusTemplate, "jobId", jobId));

        public static string NotificationMediaUsageJobStatusPublicPath(Guid jobId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Media,
                FormatGuidRoute(NotificationMediaUsageJobStatusTemplate, "jobId", jobId));
    }

    /// <summary>Route contract do Student Service sở hữu.</summary>
    public static class Students
    {
        public const string List = "/api/students";
        public const string GetByIdTemplate = "/api/students/{studentId}";

        /// <summary>Trả path nội bộ của endpoint danh sách học viên.</summary>
        public static string ListServicePath() =>
            BuildServicePath(List);

        /// <summary>Trả path public của endpoint danh sách học viên.</summary>
        public static string ListPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Student, List);

        /// <summary>Nhận student ID hợp lệ và trả path nội bộ lấy chi tiết học viên.</summary>
        public static string GetByIdServicePath(Guid studentId) =>
            BuildServicePath(
                FormatGuidRoute(GetByIdTemplate, "studentId", studentId));

        /// <summary>Nhận student ID hợp lệ và trả path Gateway lấy chi tiết học viên.</summary>
        public static string GetByIdPublicPath(Guid studentId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Student,
                FormatGuidRoute(GetByIdTemplate, "studentId", studentId));
    }

    /// <summary>Route contract do Notification Service sở hữu.</summary>
    public static class Notifications
    {
        public const string Items = "/api/notifications";
        public const string ItemByIdTemplate = "/api/notifications/{notificationId}";
        public const string Batches = "/api/notification-batches";
        public const string BatchByIdTemplate = "/api/notification-batches/{batchId}";
        public const string BatchFailedItemsTemplate = "/api/notification-batches/{batchId}/failed-items";
        public const string BatchRetryFailedTemplate = "/api/notification-batches/{batchId}/retry-failed";
        public const string BatchSnapshotStatusTemplate = "/api/notification-batches/{batchId}/snapshot-status";
        public const string BatchDeliveryStatusTemplate = "/api/notification-batches/{batchId}/delivery-status";

        public static string BatchesPublicPath() =>
            BuildPublicPath(GatewayRoutePrefixes.Notification, Batches);

        /// <summary>Trả path nội bộ của một notification batch.</summary>
        public static string BatchByIdServicePath(Guid batchId) =>
            BuildServicePath(FormatGuidRoute(BatchByIdTemplate, "batchId", batchId));

        /// <summary>Trả path Gateway của một notification batch.</summary>
        public static string BatchByIdPublicPath(Guid batchId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Notification,
                FormatGuidRoute(BatchByIdTemplate, "batchId", batchId));

        /// <summary>Trả path nội bộ của các item thất bại trong batch.</summary>
        public static string BatchFailedItemsServicePath(Guid batchId) =>
            BuildServicePath(
                FormatGuidRoute(BatchFailedItemsTemplate, "batchId", batchId));

        /// <summary>Trả path nội bộ để retry riêng các recipient thất bại của batch.</summary>
        public static string BatchRetryFailedServicePath(Guid batchId) =>
            BuildServicePath(FormatGuidRoute(BatchRetryFailedTemplate, "batchId", batchId));

        public static string BatchSnapshotStatusServicePath(Guid batchId) =>
            BuildServicePath(FormatGuidRoute(BatchSnapshotStatusTemplate, "batchId", batchId));

        public static string BatchSnapshotStatusPublicPath(Guid batchId) =>
            BuildPublicPath(GatewayRoutePrefixes.Notification,
                FormatGuidRoute(BatchSnapshotStatusTemplate, "batchId", batchId));

        public static string BatchDeliveryStatusServicePath(Guid batchId) =>
            BuildServicePath(FormatGuidRoute(BatchDeliveryStatusTemplate, "batchId", batchId));

        public static string BatchDeliveryStatusPublicPath(Guid batchId) =>
            BuildPublicPath(GatewayRoutePrefixes.Notification,
                FormatGuidRoute(BatchDeliveryStatusTemplate, "batchId", batchId));

        /// <summary>Trả path nội bộ của một notification.</summary>
        public static string ItemByIdServicePath(Guid notificationId) =>
            BuildServicePath(FormatGuidRoute(ItemByIdTemplate, "notificationId", notificationId));

        /// <summary>Trả path Gateway của một notification.</summary>
        public static string ItemByIdPublicPath(Guid notificationId) =>
            BuildPublicPath(
                GatewayRoutePrefixes.Notification,
                FormatGuidRoute(ItemByIdTemplate, "notificationId", notificationId));
    }

    /// <summary>Chuẩn hóa route thành path không có dấu <c>/</c> đầu, phù hợp cấu hình downstream/YARP.</summary>
    /// <param name="route">Route contract đầy đủ, không được rỗng.</param>
    /// <returns>Route sau khi bỏ dấu <c>/</c> đầu.</returns>
    public static string BuildServicePath(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        return route.TrimStart('/');
    }

    /// <summary>Ghép prefix Gateway và route thành public path có đúng một dấu phân cách.</summary>
    /// <param name="gatewayRoutePrefix">Prefix public, ví dụ <c>/student</c>.</param>
    /// <param name="route">Route service bắt đầu bằng hoặc không bắt đầu bằng <c>/</c>.</param>
    /// <returns>Public path bắt đầu bằng <c>/</c>.</returns>
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

    /// <summary>Thay placeholder GUID theo invariant format và chặn Guid.Empty để không sinh URL không hợp lệ.</summary>
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
