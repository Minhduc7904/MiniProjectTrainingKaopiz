namespace BuildingBlocks.Contracts.Api;

/// <summary>
/// Tập trung error code của response API để middleware và endpoint không tự tạo magic string.
/// Dùng cùng <see cref="ApiErrorMessages"/> khi tạo <c>ApiErrorResponse</c>.
/// </summary>
public static class ApiErrorCodes
{
    public const string DatabaseUnavailable = "DATABASE_UNAVAILABLE";
    public const string StorageUnavailable = "STORAGE_UNAVAILABLE";
    public const string DependencyUnavailable = "DEPENDENCY_UNAVAILABLE";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
}

/// <summary>Thông điệp an toàn có thể trả về cho client tương ứng với <see cref="ApiErrorCodes"/>.</summary>
public static class ApiErrorMessages
{
    public const string DatabaseUnavailable = "Database is temporarily unavailable.";
    public const string StorageUnavailable = "Storage is temporarily unavailable.";
    public const string DependencyUnavailable = "One or more service dependencies are temporarily unavailable.";
    public const string ServiceUnavailable = "Service is temporarily unavailable.";
    public const string UnexpectedError = "An unexpected error occurred.";
    public const string ValidationFailed = "One or more validation errors occurred.";
    public const string PayloadTooLarge = "The request payload exceeds the allowed size.";
}

/// <summary>Tên HTTP header dùng chung giữa middleware, HTTP client và API Gateway.</summary>
public static class ApiHeaderNames
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string ActorType = "X-Actor-Type";
    public const string ActorId = "X-Actor-Id";
}

/// <summary>Tên header metadata được gắn vào message RabbitMQ.</summary>
public static class MessagingHeaderNames
{
    public const string SourceService = "X-Source-Service";
}

/// <summary>Đường dẫn hạ tầng dùng chung cho mọi service.</summary>
public static class ApiPaths
{
    public const string Health = "/health";
    public const string OpenApiDocument = "/swagger/v1/swagger.json";
}

/// <summary>Giá trị trạng thái chuẩn trong response health check.</summary>
public static class HealthStatusValues
{
    public const string Healthy = "healthy";
    public const string Unhealthy = "unhealthy";
}

/// <summary>Định danh service dùng cho cấu hình endpoint, messaging và observability.</summary>
public static class ServiceNames
{
    public const string Admin = "admin-service";
    public const string Course = "course-service";
    public const string Student = "student-service";
    public const string Media = "media-service";
    public const string Notification = "notification-service";
    public const string Scheduler = "scheduler-service";
    public const string Gateway = "lms-api-gateway";
}

/// <summary>Prefix public của từng service khi đi qua API Gateway.</summary>
public static class GatewayRoutePrefixes
{
    public const string Admin = "/admin";
    public const string Course = "/course";
    public const string Student = "/student";
    public const string Media = "/media";
    public const string Notification = "/notification";
    public const string Scheduler = "/scheduler";
}

/// <summary>Tên các section cấu hình hạ tầng được BuildingBlocks đọc.</summary>
public static class ConfigurationSectionNames
{
    public const string ServiceEndpoints = "ServiceEndpoints";
    public const string Cors = "Cors";
}

/// <summary>Tên CORS policy đã đăng ký để API gọi nhất quán trong pipeline.</summary>
public static class CorsPolicyNames
{
    public const string Frontend = "frontend";
}
