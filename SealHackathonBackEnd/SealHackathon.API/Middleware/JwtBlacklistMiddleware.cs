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
                    await context.Response.WriteAsync(
                        """{"success":false,"message":"Token đã bị thu hồi. Vui lòng đăng nhập lại.","data":null}"""
                    );
                    return;
                }
            }

            await _next(context);
        }
    }
}
