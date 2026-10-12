using System.Net;
using System.Text.Json;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;

namespace ShareStudy.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
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
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình xử lý request: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        List<string>? errors = null;

        switch (exception)
        {
            case ValidationException valEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = valEx.Message;
                errors = valEx.Errors;
                break;

            case NotFoundException notFoundEx:
                statusCode = (int)HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;

            case UnauthorizedException unauthEx:
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = unauthEx.Message;
                break;

            case ForbiddenException forbiddenEx:
                statusCode = (int)HttpStatusCode.Forbidden;
                message = forbiddenEx.Message;
                break;

            case BadRequestException badReqEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = badReqEx.Message;
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Đã xảy ra lỗi máy chủ nội bộ. Vui lòng thử lại sau.";
                if (_env.IsDevelopment())
                {
                    errors = new List<string> { exception.Message, exception.StackTrace ?? string.Empty };
                }
                break;
        }

        context.Response.StatusCode = statusCode;
        var response = ApiResponse<object>.FailureResponse(message, statusCode, errors);

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
