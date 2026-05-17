namespace SealHackathon.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================
            // 1. CẤU HÌNH CORS (PHẢI ĐẶT TRƯỚC builder.Build)
            // ==========================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    // Cho phép ReactJS gọi tới (AllowAnyOrigin), 
                    // gửi mọi loại dữ liệu (AllowAnyHeader), 
                    // và dùng mọi phương thức GET, POST, PUT, DELETE (AllowAnyMethod)
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Cấu hình môi trường chạy
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // ==========================================
            // 2. KÍCH HOẠT CORS (BẮT BUỘC ĐẶT Ở VỊ TRÍ NÀY)
            // Phải đặt SAU UseHttpsRedirection và TRƯỚC UseAuthorization
            // ==========================================
            app.UseCors("AllowReactApp");

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}