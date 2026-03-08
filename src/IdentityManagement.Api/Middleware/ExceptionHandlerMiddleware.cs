using System.Net;
using System.Text.Json;
using IdentityManagement.Application.Common;
using Microsoft.AspNetCore.Http;

namespace IdentityManagement.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var (statusCode, response) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ApiResponse.Fail(exception.Message)),
            KeyNotFoundException => (HttpStatusCode.NotFound, ApiResponse.Fail(exception.Message)),
            ArgumentException => (HttpStatusCode.BadRequest, ApiResponse.Fail(exception.Message)),
            InvalidOperationException => (HttpStatusCode.BadRequest, ApiResponse.Fail(exception.Message)),
            _ => (HttpStatusCode.InternalServerError,
                ApiResponse.Fail(_env.IsDevelopment() ? exception.Message : "An error occurred."))
        };

        context.Response.StatusCode = (int)statusCode;
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
