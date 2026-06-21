using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Interfaces.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SealHackathon.API.BackgroundServices
{
    public class UnverifiedAccountCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnverifiedAccountCleanupService> _logger;

        public UnverifiedAccountCleanupService(IServiceProvider serviceProvider, ILogger<UnverifiedAccountCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Service:Delete account not accept pending.");

            // Vòng lặp chạy liên tục cho đến khi ứng dụng tắt
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Tạo một scope mới để lấy ra các dịch vụ (như IUnitOfWork)
                    // Vì BackgroundService là Singleton, còn IUnitOfWork là Scoped
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var repo = uow.GetRepository<Account>();

                        // Lấy thời điểm hiện tại
                        var now = DateTime.UtcNow;

                        // Tìm những tài khoản:
                        // 1. Role là "Pending" (chưa xác thực)
                        // 2. Có thời hạn TokenExpiresAt nhỏ hơn thời gian hiện tại (Đã quá hạn)
                        // 3. Chưa bị xóa mềm
                        var expiredAccounts = await repo.GetAllAsync(a => 
                            a.SystemRole == "Pending" && 
                            a.TokenExpiresAt != null && 
                            a.TokenExpiresAt < now && 
                            !a.IsDeleted);

                        if (expiredAccounts.Any())
                        {
                            _logger.LogInformation($"Phát hiện {expiredAccounts.Count} tài khoản ảo đã quá hạn. Đang tiến hành dọn dẹp...");

                            foreach (var account in expiredAccounts)
                            {
                                // Xóa vĩnh viễn (Hard Delete) để làm sạch Database hoàn toàn
                                repo.Delete(account);
                            }

                            await uow.SaveChangesAsync();
                            _logger.LogInformation("Dọn rác hoàn tất!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Có lỗi xảy ra trong quá trình dọn dẹp tài khoản ảo.");
                }

                // Tạm dừng 10 phút trước khi quét lại (Có thể chỉnh thành 1 giờ hay 1 ngày tùy ý)
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}
