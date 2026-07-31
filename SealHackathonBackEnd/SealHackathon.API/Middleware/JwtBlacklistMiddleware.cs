using SealHackathon.Infrastructure.Services;

namespace SealHackathon.API.Middleware
{
    public class JwtBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtBlacklistMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();

                if (AuthService.IsTokenBlacklisted(token))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    
                    var apiResponse = SealHackathon.Application.Common.Responses.ApiResponse<string>.FailResult(
                        "Token đã bị thu hồi. Vui lòng đăng nhập lại.");
                        
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(apiResponse));
                    return;
                }
            }

            await _next(context);
        }
    }
}
