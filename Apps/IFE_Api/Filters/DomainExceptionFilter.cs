using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Store_API.Filters;

public sealed class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not Exception ex || ex is FluentValidation.ValidationException)
            return;

        context.Result = new BadRequestObjectResult(new { message = ex.Message });
        context.ExceptionHandled = true;
    }
}
