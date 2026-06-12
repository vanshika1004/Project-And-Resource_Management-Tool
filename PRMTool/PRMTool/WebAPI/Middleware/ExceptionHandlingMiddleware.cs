using System.Net;
using System.Text.Json;

namespace WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

    private async Task HandleExceptionAsync(
        HttpContext context, Exception exception)
    {
        _logger.LogError(exception,
            "Unhandled exception: {Message}",
            exception.Message);

        var statusCode = exception switch
        {
            KeyNotFoundException
                => HttpStatusCode.NotFound,
            UnauthorizedAccessException
                => HttpStatusCode.Forbidden,
            ArgumentException
                => HttpStatusCode.BadRequest,
            InvalidOperationException
                => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new
        {
            success = false,
            message = exception.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(
            response, jsonOptions);
    }
}
