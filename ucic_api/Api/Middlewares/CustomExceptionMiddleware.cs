using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;
using Application.Common.Exceptions;
using System.Text.Json;

namespace Api.Middlewares
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var loggedInUser = context.User?.FindFirst(ClaimTypes.Name)?.Value;
                var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

                //using (Serilog.Context.LogContext.PushProperty("LoggedInUser", loggedInUser))
                //{
                    await _next(context);
                //}
                //await _next(context); 
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
                    response = new
                    {
                        Success = false,
                        Message = notFoundEx.Message,
                        Details = (object?)null
                    };
                    _logger.LogWarning("Not Found Exception for user {User}: {Message}", loggedInUser, notFoundEx.Message);
                    break;

                case BadRequestException badRequestEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new
                    {
                        Success = false,
                        Message = badRequestEx.Message,
                        Details = (object?)null
                    };
                    _logger.LogWarning("Bad Request Exception for user {User}: {Message}", loggedInUser, badRequestEx.Message);
                    break;

                case ForbiddenAccessException forbiddenEx:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    response = new
                    {
                        Success = false,
                        Message = "Access forbidden.",
                        Details = (object?)null
                    };
                    _logger.LogWarning("Forbidden Access Exception for user {User}: {Message}", loggedInUser, forbiddenEx.Message);
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new
                    {
                        Success = false,
                        Message = "Validation failed.",
                        Details = (object?)validationEx.Errors
                    };
                    _logger.LogWarning("Validation Exception for user {User}: {Errors}", loggedInUser, JsonSerializer.Serialize(validationEx.Errors));
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response = new
                    {
                        Success = false,
                        Message = keyNotFoundEx.Message,
                        Details = (object?)null
                    };
                    _logger.LogWarning("Key Not Found Exception for user {User}: {Message}", loggedInUser, keyNotFoundEx.Message);
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response = new
                    {
                        Success = false,
                        Message = "An unexpected error occurred. Please try again later.",
                        Details = (object?)null
                    };
                    _logger.LogError(exception, "Unhandled exception for user {User}: {Message}", loggedInUser, exception.Message);
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}

