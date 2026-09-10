using ClientService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ClientService.Api.Middleware;

/// <summary>
/// Central exception-to-ProblemDetails mapping (RFC 7807, .NET 8
/// IExceptionHandler). Controllers stay free of try/catch and the API exposes
/// one consistent error contract:
///   - ClienteNotFoundException           -> 404
///   - InvalidOperationException (unique)  -> 409
///   - ArgumentException/FormatException   -> 400 (domain guard clauses)
///   - anything else                       -> 500 (generic, logged)
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ClienteNotFoundException => (
                StatusCodes.Status404NotFound,
                "Client not found",
                exception.Message),
            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "Request conflicts with the current state",
                exception.Message),
            ArgumentException or FormatException => (
                StatusCodes.Status400BadRequest,
                "Invalid request data",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "Check the service logs for details.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing the request");
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}"
        }, cancellationToken);

        return true;
    }
}