using System.Net;
using System.Security.Claims;
using System.Text;
using Dtos.ExceptionDto;
using Exceptions;
using Newtonsoft.Json;

namespace Middlewares;

public class ExceptionLoggerMiddleware
{
    private readonly ILogger<ExceptionLoggerMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionLoggerMiddleware(RequestDelegate next, ILogger<ExceptionLoggerMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var httpObject = JsonConvert.SerializeObject(new
        {
            User = httpContext.User.FindFirstValue("id"),
            Url = httpContext.Request.Path,
            httpContext.Request.Method
        });
        try
        {
            object? requestBody = null;
            using (var stream = new MemoryStream())
            {
                httpContext.Request.EnableBuffering();
                await httpContext.Request.Body.CopyToAsync(stream);
                requestBody = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(stream.ToArray()));
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            _logger.LogInformation("<START> HTTP & User: {} - Body: {}",
                httpObject, JsonConvert.SerializeObject(requestBody));
            await _next(httpContext);

            object? responseBody = null;
            using (var stream = new MemoryStream())
            {
                await httpContext.Request.Body.CopyToAsync(stream);
                responseBody = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(stream.ToArray()));
            }

            _logger.LogInformation("<SUCCESS> HTTP & User: {} - Body: {}",
                httpObject, JsonConvert.SerializeObject(responseBody));
        }
        catch (ApiInputException ex)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ResponseErrorDto<DataFailDto> errorResponse = new()
            {
                ErrorCode = ex.ErrorCode,
                ErrorDetail = ex.ErrorDetail
            };
            _logger.LogError("<FAIL> ApiInputException HTTP & User: {} - Body: {}", httpObject,
                JsonConvert.SerializeObject(errorResponse));
            await HandleExceptionAsync(httpContext, errorResponse);
        }
        catch (ApiValidateException ex)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ResponseErrorDto<List<DataFailDto>> errorResponse = new()
            {
                ErrorCode = ex.ErrorCode,
                ErrorDetail = ex.ErrorDetail
            };
            _logger.LogError("<FAIL> ApiValidateException HTTP & User: {} - Body: {}", httpObject,
                JsonConvert.SerializeObject(errorResponse));
            await HandleExceptionAsync(httpContext, errorResponse);
        }
        catch (Exception ex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            _logger.LogError("<FAIL> Something went wrong: {ex}", ex.ToString());
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, object response)
        => await context.Response.WriteAsync(JsonConvert.SerializeObject(response));

}
