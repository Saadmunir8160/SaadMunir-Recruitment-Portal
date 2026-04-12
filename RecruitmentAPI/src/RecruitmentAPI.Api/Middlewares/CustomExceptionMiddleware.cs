using System.Net;
using System.Security.Claims;
using System.Text.Json;
using RecruitmentAPI.Application.Common.Exceptions;

namespace RecruitmentAPI.Api.Middlewares;

public class CustomExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionMiddleware> _logger;

    public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var loggedInUser = context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
            await HandleExceptionAsync(context, ex, loggedInUser);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string loggedInUser)
    {
        context.Response.ContentType = "application/json";

        object response;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = new { Success = false, Message = notFoundEx.Message };
                _logger.LogWarning("Not Found for user {User}: {Message}", loggedInUser, notFoundEx.Message);
                break;

            case BadRequestException badRequestEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Success = false, Message = badRequestEx.Message };
                _logger.LogWarning("Bad Request for user {User}: {Message}", loggedInUser, badRequestEx.Message);
                break;

            case ForbiddenAccessException forbiddenEx:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                response = new { Success = false, Message = "Access forbidden." };
                _logger.LogWarning("Forbidden for user {User}: {Message}", loggedInUser, forbiddenEx.Message);
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response = new { Success = false, Message = validationEx.Message, Errors = validationEx.Errors };
                _logger.LogWarning("Validation error for user {User}: {Message}", loggedInUser, validationEx.Message);
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new { Success = false, Message = "An unexpected error occurred." };
                _logger.LogError(exception, "Unhandled exception for user {User}", loggedInUser);
                break;
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
