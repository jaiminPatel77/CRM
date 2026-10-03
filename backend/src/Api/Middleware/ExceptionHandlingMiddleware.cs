using Crm.Api.Models;
using Crm.Application.Common.Exceptions;
using Crm.Application.Common.Models;
using Crm.Domain.Consts;
using System.Text.Json;

namespace Crm.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns appropriate HTTP responses with structured error messages.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (ForbiddenAccessException ex)
        {
            _logger.LogWarning(ex, "Forbidden access attempt");
            await HandleForbiddenExceptionAsync(context);
        }
        catch (CrmApplicationException ex)
        {
            _logger.LogWarning(ex, "Application exception: {ErrorId} - {Message}", ex.ErrorId, ex.Message);
            await HandleApplicationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleUnhandledExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var response = new ApiBadRequestResponse(
            EnumEntityType.COMMON,
            EnumEntityEvents.COMMON_VALIDATION_ERROR,
            ex.Errors.SelectMany(e => e.Value).ToArray());

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
    }

    private static async Task HandleNotFoundExceptionAsync(HttpContext context, NotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";

        var response = new ApiBadRequestResponse(
            EnumEntityType.COMMON,
            EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND,
            ex.Message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
    }

    private static async Task HandleForbiddenExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        var response = new ApiBadRequestResponse(
            EnumEntityType.COMMON,
            EnumEntityEvents.COMMON_ACCESS_DENIED,
            "Access denied.");

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
    }

    private static async Task HandleApplicationExceptionAsync(HttpContext context, CrmApplicationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var response = new ApiBadRequestResponse(
            EnumEntityType.COMMON,
            EnumEntityEvents.COMMON_GET_EXCEPTION,
            ex.Message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
    }

    private static async Task HandleUnhandledExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        // In production, don't expose exception details
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        var message = isDevelopment ? ex.Message : "An unexpected error occurred. Please try again later.";

        var response = new ApiBadRequestResponse(
            EnumEntityType.COMMON,
            EnumEntityEvents.COMMON_GET_EXCEPTION,
            message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
    }

    private static JsonSerializerOptions GetJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

/// <summary>
/// Extension method to register the middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
