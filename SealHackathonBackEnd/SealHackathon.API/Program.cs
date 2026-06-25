using DbUp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SealHackathon.API.Middleware;
using SealHackathon.Domain.Interfaces.Repositories;
using SealHackathon.Infrastructure.Data;
using SealHackathon.Infrastructure.Repositories;
using System.Text;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Infrastructure.Services;
using SealHackathon.Application.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = Encoding.UTF8;

// ==========================================
// 1. CORS — FE Vite (localhost:5173)
// ==========================================
var frontendBaseUrl = builder.Configuration["AppSettings:FrontendBaseUrl"]
    ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins(frontendBaseUrl)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ==========================================
// 2. JWT Authentication
// ==========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing.");

builder.Services.AddAuthentication(options =>
{
    // DefaultAuthenticateScheme: scheme dùng để xác thực request đến
    // DefaultChallengeScheme: scheme dùng khi request bị từ chối (trả 401)
    // Cả 2 đều set là JwtBearer — nghĩa là hệ thống dùng JWT làm cơ chế auth duy nhất
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Bật kiểm tra chữ ký — đảm bảo token không bị giả mạo
        ValidateIssuerSigningKey = true,

        // IssuerSigningKey: key dùng để verify chữ ký của token
        // SymmetricSecurityKey: dùng cùng 1 key để ký và verify (symmetric)
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        // Bật kiểm tra Issuer — token phải được ký bởi đúng server
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        // Bật kiểm tra Audience — token phải được tạo cho đúng client
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        // Bật kiểm tra thời hạn token
        ValidateLifetime = true,

        // ClockSkew: độ lệch thời gian cho phép giữa server và client
        // Mặc định là 5 phút — set về 0 để token hết hạn chính xác theo ExpirationInMinutes
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase cho tất cả response JSON — Frontend JS expect format này
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
            
        // Fix lỗi hiển thị tiếng Việt (unicode) trong JSON response
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger hiểu JWT — cho phép test API có auth trực tiếp trên Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    // Sửa lỗi trùng tên DTO trong Swagger
    options.CustomSchemaIds(type => type.FullName);

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token. Ví dụ: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================
// 3. ĐĂNG KÝ DEPENDENCY INJECTION (DI) 
// ==========================================
// Đăng ký DbContext với chuỗi kết nối lấy từ appsettings.json
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddDbContext<SealDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlOptions => sqlOptions.UseCompatibilityLevel(120));
});

// Đăng ký UnitOfWork với vòng đời Scoped
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IRoundService, RoundService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<ICriterionService, CriterionService>();
builder.Services.AddScoped<ICriterionTemplateService, CriterionTemplateService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPrizeService, PrizeService>();

builder.Services.AddScoped<INotificationService, NotificationService>();
//=======
builder.Services.AddScoped<IAuditLogService, AuditLogService>();


// Đăng ký Background Service - chạy ngầm
builder.Services.AddHostedService<SealHackathon.API.BackgroundServices.UnverifiedAccountCleanupService>();

var app = builder.Build();

// ==========================================
// 4. DbUp
// ==========================================
RunDbUp(builder.Configuration);

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();
app.UseCors("AllowReactApp");


// QUAN TRỌNG: UseAuthentication phải đứng TRƯỚC UseAuthorization
// Lý do: Authentication xác định "bạn là ai", Authorization xác định "bạn được làm gì"
// Nếu đảo ngược thứ tự, Authorization không biết user là ai → mọi request đều bị từ tionchối
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void RunDbUp(IConfiguration configuration)
{
    string? connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("LỖI: Không tìm thấy DefaultConnection!");
        Console.ResetColor();
        return;
    }

// EnsureDatabase.For.SqlDatabase(connectionString); // Bỏ comment dòng này trên MonsterASP vì DB đã được tạo sẵn, và không có quyền truy cập master db.

    var upgrader = DeployChanges.To
        .SqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly())
        .WithVariablesDisabled()
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
