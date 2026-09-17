using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Exceptions;

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
            
            case InvalidOperationException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Invalid operation";
                problemDetails.Detail = exception.Message;
                break;
            
            case ArgumentException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Invalid argument";
                problemDetails.Detail = exception.Message;
                break;

            case EntityInUseException:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Entity in use";
                problemDetails.Detail = exception.Message;
                break;

            case DuplicateValueException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Duplicate value";
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