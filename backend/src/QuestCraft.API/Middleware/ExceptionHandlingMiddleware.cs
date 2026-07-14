using System.Net;
using System.Text.Json;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Models;
using ValidationException = QuestCraft.Application.Common.Exceptions.ValidationException;

namespace QuestCraft.API.Middleware;

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
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException ex => (HttpStatusCode.BadRequest, ApiResponse<object>.Fail("Validasiya xətası.", ex.Errors)),
            BadRequestException ex => (HttpStatusCode.BadRequest, ApiResponse<object>.Fail(ex.Message)),
            NotFoundException ex => (HttpStatusCode.NotFound, ApiResponse<object>.Fail(ex.Message)),
            ConflictException ex => (HttpStatusCode.Conflict, ApiResponse<object>.Fail(ex.Message)),
            UnauthorizedException ex => (HttpStatusCode.Unauthorized, ApiResponse<object>.Fail(ex.Message)),
            ForbiddenException ex => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(ex.Message)),
            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail("Daxili server xətası baş verdi.")),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Gözlənilməyən xəta: {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        }));
    }
}
