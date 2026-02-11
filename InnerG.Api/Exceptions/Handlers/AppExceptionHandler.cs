using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InnerG.Api.Exceptions.Handlers
{
    internal sealed class AppExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AppExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not AppException appException)
            return false;

        logger.LogWarning(exception, "Application exception");

        httpContext.Response.StatusCode = appException.StatusCode;

        var problemDetails = new ProblemDetails
        {
            Title = "Application error",
            Status = appException.StatusCode,
            Detail = appException.Message,
            Instance = httpContext.Request.Path
        };

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }
}

}