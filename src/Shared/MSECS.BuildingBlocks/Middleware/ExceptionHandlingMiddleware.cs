using System.Net;
using System.Text.Json;
using MSECS.SharedKernel.Exceptions;
using Serilog;

namespace MSECS.BuildingBlocks.Middleware;

/// <summary>
/// Central exception-to-HTTP-response translator. Every MSECS API registers this
/// as the first middleware so handlers can throw domain exceptions freely and the
/// caller always receives a consistent ProblemDetails-shaped payload.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var cid)
            ? cid?.ToString()
            : null;

        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ValidationAppException => (HttpStatusCode.BadRequest, "Validation failed"),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Access denied"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            DeviceCommunicationException => (HttpStatusCode.BadGateway, "Device communication failure"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            Log.Error(exception, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
        }
        else
        {
            Log.Warning(exception, "Handled exception {Title}. CorrelationId={CorrelationId}", title, correlationId);
        }

        var problem = new
        {
            type = $"https://httpstatuses.io/{(int)statusCode}",
            title,
            status = (int)statusCode,
            correlationId,
            errors = exception is ValidationAppException vex ? vex.Errors : null,
            detail = statusCode == HttpStatusCode.InternalServerError ? null : exception.Message
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseMsecsExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
