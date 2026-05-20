using DbUp;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text;

namespace SealHackathon.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Console.OutputEncoding = Encoding.UTF8; // Tiếng Việt có dấu trong console

            // ==========================================
            // 1. CẤU HÌNH CORS (KHÔNG DÙNG LAMBDA)
            // ==========================================
            builder.Services.AddCors(ConfigureCorsOptions);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Chạy DbUp
            RunDbUp(builder.Configuration);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowReactApp");

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        private static void ConfigureCorsOptions(CorsOptions options)
        {
            options.AddPolicy("AllowReactApp", ConfigureCorsPolicy);
        }

        private static void ConfigureCorsPolicy(CorsPolicyBuilder policy)
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }

        private static void RunDbUp(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("LỖI: Không tìm thấy DefaultConnection!");
                Console.ResetColor();
                return;
            }

            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("LỖI KHI CẬP NHẬT DATABASE: " + result.Error);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("DATABASE ĐÃ ĐƯỢC ĐỒNG BỘ THÀNH CÔNG!");
                Console.ResetColor();
            }
        }
    }
}
