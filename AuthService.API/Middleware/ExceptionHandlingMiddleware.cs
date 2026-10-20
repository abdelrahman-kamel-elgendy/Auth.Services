using System.Net;
using System.Text.Json;
using AuthService.API.Exceptions;
using AuthService.API.Models;

namespace AuthService.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    IWebHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly IWebHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        ApiResponse<object> apiResponse = new(false, null, null);

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                apiResponse.Message = validationEx.Message;
                apiResponse.ValidationErrors = validationEx.Errors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList());
                break;

            case BadRequestException badRequestEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                apiResponse.Message = badRequestEx.Message;
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                apiResponse.Message = notFoundEx.Message;
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                apiResponse.Message = unauthorizedEx.Message;
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                apiResponse.Message = forbiddenEx.Message;
                break;

            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                apiResponse.Message = conflictEx.Message;
                break;

            case NotImplementedException notImplEx:
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                apiResponse.Message = notImplEx.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                apiResponse.Message = "An internal server error occurred. Please try again later.";
                if (_env.IsDevelopment())
                    apiResponse.Errors = [exception.Message, exception.StackTrace ?? string.Empty];
                break;
        }

        await response.WriteAsync(JsonSerializer.Serialize(
            apiResponse,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}