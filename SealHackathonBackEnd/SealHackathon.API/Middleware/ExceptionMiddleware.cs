using Microsoft.AspNetCore.Http;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Domain.Exceptions;
using System.Text.Json;

namespace SealHackathon.API.Middleware
{
    public class ExceptionMiddleware
    {
        // _next: đại diện cho middleware tiếp theo trong pipeline
        // Nếu không có lỗi → gọi _next để request đi tiếp vào Controller
        private readonly RequestDelegate _next;
        
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
       

        // Hàm này được .NET tự động gọi mỗi khi có request đến
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho request đi tiếp vào Controller
                await _next(context);
            }
            catch (AppException ex)
            {
                // Bắt được custom exception của chúng ta
                await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                // Ghi lỗi ra Console server — chỉ dev thấy, client không thấy
                // Bắt được exception không mong muốn — lỗi hệ thống
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, 500, "Đã có lỗi xảy ra. Vui lòng thử lại sau.");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            // Thiết lập response trả về dạng JSON UTF-8 để message tiếng Việt không bị lỗi ký tự.
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = statusCode;

            var response = ApiResponse<object>.FailResult(message);

            // Serialize object thành JSON string rồi ghi vào response
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await context.Response.WriteAsync(json);
        }
    }
}
