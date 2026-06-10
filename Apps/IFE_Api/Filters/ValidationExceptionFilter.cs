using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Store_API.Filters;

public sealed class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ValidationException validationException)
            return;

        context.Result = new BadRequestObjectResult(new
        {
            message = "Validation failed.",
            errors = validationException.Errors.Select(e => e.ErrorMessage)
        });
        context.ExceptionHandled = true;
    }
}
