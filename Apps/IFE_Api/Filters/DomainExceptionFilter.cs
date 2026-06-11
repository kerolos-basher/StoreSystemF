using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Store_API.Filters;

public sealed class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is FluentValidation.ValidationException)
            return;

        if (context.Exception is StoreException storeEx)
        {
            context.Result = new ObjectResult(new { message = storeEx.StoreExceptionMessage })
            {
                StatusCode = 402
            };
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is Exception ex)
        {
            context.Result = new BadRequestObjectResult(new { message = ex.Message });
            context.ExceptionHandled = true;
        }
    }
}
