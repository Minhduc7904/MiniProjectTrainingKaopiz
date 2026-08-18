using BuildingBlocks.Presentation.Middleware;

namespace BuildingBlocks.Presentation.Extensions;

/// <summary>Đăng ký middleware dùng chung theo đúng thứ tự cho ASP.NET Core API.</summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Thêm correlation middleware trước exception middleware để mọi error response và log đều có trace ID.
    /// Input là application builder; output là builder để tiếp tục fluent pipeline.
    /// </summary>
    public static IApplicationBuilder UseSharedApiMiddleware(this IApplicationBuilder app)
    {
        // Thứ tự là bắt buộc: exception handler phải nhìn thấy TraceIdentifier do middleware đầu tạo/forward.
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ApiExceptionHandlingMiddleware>();

        return app;
    }
}
