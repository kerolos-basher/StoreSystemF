// Ignore Spelling: Store API app
using Infrastructure.Services.LogFile;
using Microsoft.AspNetCore.Diagnostics;
using Resources;
using System.Net;
namespace Store_API.Extensions;

public static class ExceptionMiddleWareExtensions
{
    public static void ConfigureExceptionHandler(this WebApplication app, LogFileService logger)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var exception = contextFeature.Error;
                    var statusCode = (int)HttpStatusCode.InternalServerError;
                    var message = ExceptionMessage.UnHandledException.ToString();

                    logger.LogExceptionString($"From UseExceptionHandler MiddleWare:-\n {contextFeature.Error}");

                    if (exception is StoreException StoreException)
                    {
                        message = StoreException.StoreExceptionMessage.Trim();
                    }

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(message);
                }
            });
        });
    }
}
