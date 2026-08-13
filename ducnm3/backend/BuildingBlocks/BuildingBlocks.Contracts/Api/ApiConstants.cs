namespace BuildingBlocks.Contracts.Api;

public static class ApiErrorCodes
{
    public const string DatabaseUnavailable = "DATABASE_UNAVAILABLE";
    public const string StorageUnavailable = "STORAGE_UNAVAILABLE";
    public const string DependencyUnavailable = "DEPENDENCY_UNAVAILABLE";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
    public const string ValidationFailed = "VALIDATION_FAILED";
}

public static class ApiErrorMessages
{
    public const string DatabaseUnavailable = "Database is temporarily unavailable.";
    public const string StorageUnavailable = "Storage is temporarily unavailable.";
    public const string DependencyUnavailable = "One or more service dependencies are temporarily unavailable.";
    public const string ServiceUnavailable = "Service is temporarily unavailable.";
    public const string UnexpectedError = "An unexpected error occurred.";
    public const string ValidationFailed = "One or more validation errors occurred.";
}

public static class ApiHeaderNames
{
    public const string CorrelationId = "X-Correlation-Id";
}

public static class ApiPaths
{
    public const string Health = "/health";
    public const string OpenApiDocument = "/swagger/v1/swagger.json";
}

public static class HealthStatusValues
{
    public const string Healthy = "healthy";
    public const string Unhealthy = "unhealthy";
}

public static class ServiceNames
{
    public const string Course = "course-service";
    public const string Student = "student-service";
    public const string Media = "media-service";
    public const string Notification = "notification-service";
    public const string Scheduler = "scheduler-service";
    public const string Gateway = "lms-api-gateway";
}

public static class GatewayRoutePrefixes
{
    public const string Course = "/course";
    public const string Student = "/student";
    public const string Media = "/media";
    public const string Notification = "/notification";
    public const string Scheduler = "/scheduler";
}

public static class ConfigurationSectionNames
{
    public const string ServiceEndpoints = "ServiceEndpoints";
}
