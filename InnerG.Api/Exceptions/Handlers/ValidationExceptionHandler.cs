using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InnerG.Api.Exceptions;

namespace InnerG.Api.Exceptions.Handlers
{

    internal sealed class ValidationExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ValidationExceptionHandler> logger
    ) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            IDictionary<string, string[]>? errors = exception switch
            {
                FluentValidation.ValidationException fvEx =>
                    fvEx.Errors
                        .GroupBy(e => e.PropertyName.ToLowerInvariant())
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()),

                ValidationException appEx => appEx.Errors,

                _ => null
            };

            if (errors is null) return false;

            logger.LogDebug(
                "Validation failed at {Path}: {@Errors}",
                context.Request.Path,
                errors);

            var problemDetails = new ProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["errors"] = errors;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            return await problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails
                });
        }
    }
}