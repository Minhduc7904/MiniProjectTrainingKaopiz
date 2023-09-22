using Constant;
using Dtos.ExceptionDto;
using Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Middlewares;

public class ValidationFilterAttribute : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var errors = context.ModelState.Select(error => new DataFailDto
        {
            Field = error.Key,
            Error = error.Value?.Errors.Select(modelError => modelError.ErrorMessage).ToList()
        }).ToList();
        throw new ApiValidateException(ErrorCode.VALIDATION_FAIL, errors);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
