using AccountService.Application.Exceptions;
using AccountService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Api.Middleware;

/// <summary>
/// Single exception pipeline for the account service (spec: "manejo de
/// excepciones"). Maps domain/application exceptions to ProblemDetails:
///   - CuentaNotFoundException       -> 404
///   - SaldoInsuficienteException    -> 400 (F3: "Saldo no disponible")
///   - InvalidOperationException     -> 409 (e.g. duplicate account number)
///   - ArgumentException/FormatException -> 400 (invalid payloads/enums)
///   - anything else                 -> 500 (logged, generic details)
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
        var (statusCode, title) = exception switch
        {
            CuentaNotFoundException => (StatusCodes.Status404NotFound, "Account not found"),
            SaldoInsuficienteException => (StatusCodes.Status400BadRequest, "Business rule violation"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            ArgumentException or FormatException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception.");

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}