using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Presentation.Middleware;

public sealed partial class ApiExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionHandlingMiddleware> logger,
    IOptions<JsonOptions> jsonOptions)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException exception) when (!context.Response.HasStarted)
        {
            await WriteErrorAsync(
                context,
                exception.StatusCode,
                exception.ErrorCode,
                exception.SafeMessage,
                exception.Details);
        }
        catch (BadHttpRequestException exception) when (!context.Response.HasStarted)
        {
            ApiLog.InvalidRequest(logger, exception, context.TraceIdentifier);
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.ValidationFailed,
                ApiErrorMessages.ValidationFailed);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            ApiLog.UnhandledException(logger, exception, context.TraceIdentifier);
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ApiErrorCodes.UnexpectedError,
                ApiErrorMessages.UnexpectedError);
        }
    }

    private async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message,
        IReadOnlyList<ApiErrorDetail>? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            ApiResponseFactory.Error(errorCode, message, context.TraceIdentifier, details),
            jsonOptions.Value.SerializerOptions,
            context.RequestAborted);
    }

    private static partial class ApiLog
    {
        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Error,
            Message = "Unhandled API exception. TraceId: {TraceId}")]
        public static partial void UnhandledException(
            ILogger logger,
            Exception exception,
            string traceId);

        [LoggerMessage(
            EventId = 1002,
            Level = LogLevel.Debug,
            Message = "Invalid API request. TraceId: {TraceId}")]
        public static partial void InvalidRequest(
            ILogger logger,
            Exception exception,
            string traceId);
    }
}
