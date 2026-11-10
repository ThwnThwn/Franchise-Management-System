using EngagementService.Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EngagementService.Infrastructure.Persistences.BackgroundJob
{
    public class CleanupPointLogJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanupPointLogJob> _logger;

        public CleanupPointLogJob(IServiceScopeFactory scopeFactory, ILogger<CleanupPointLogJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup PointLog Job đang khởi động và thực hiện dọn dẹp lần đầu...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Thực hiện dọn dẹp NGAY LẬP TỨC khi start app hoặc khi bắt đầu chu kỳ mới
                    _logger.LogInformation("Bắt đầu tiến trình xóa PointLog quá hạn tại: {Time}", DateTimeOffset.UtcNow);
                    await DoCleanup();

                    // 2. Sau khi dọn xong, mới tính toán thời gian đợi đến ngày 1 tháng sau
                    var now = DateTimeOffset.UtcNow;
                    var nextRun = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1);
                    var delay = nextRun - now;

                    _logger.LogInformation("Đã hoàn thành. Lần dọn dẹp tiếp theo sẽ diễn ra sau: {Delay} (Dự kiến: {NextRun})", delay, nextRun);

                    // 3. Nghỉ ngơi cho đến tháng sau
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Job dọn dẹp đã bị hủy bỏ.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong Job dọn dẹp. Sẽ thử lại sau 1 giờ.");
                    // Nếu lỗi, đợi 1 khoảng ngắn (ví dụ 1 tiếng) rồi thử lại để tránh loop vô tận làm treo CPU
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task DoCleanup()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<EngagementDbContext>();
                var now = DateTimeOffset.UtcNow;

                // Lấy danh sách các logs đã quá hạn (ExpiredAt < Hiện tại)
                // Lưu ý: Chỉ xóa các loại record phát sinh điểm có hạn dùng
                var expiredLogs = dbContext.PointLogs
                    .Where(l => l.ExpiredAt.HasValue && l.ExpiredAt < now);

                int deletedCount = await expiredLogs.ExecuteDeleteAsync();

                _logger.LogInformation("Đã xóa thành công {Count} bản ghi PointLog quá hạn.", deletedCount);
            }
        }
    }
}
