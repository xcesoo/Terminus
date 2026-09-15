using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Terminus.WebApi.Middlewares;

public class GlobalExceptionsHandler(ILogger<GlobalExceptionsHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, $"Exception occured: {exception.Message}");

        var problemDetails = new ProblemDetails { Instance = httpContext.Request.Path };

        switch (exception)
        {
            case KeyNotFoundException:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Key not found";
                problemDetails.Detail = exception.Message;
                break;
            
            default:
                logger.LogError(
                    exception,
                    "Unhandled exception while processing {Path}",
                    httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal server error";
                problemDetails.Detail = "An unexpected exception occured";
                break;
        }
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}