using EngagementService.Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace EngagementService.API.BackgroundJob
{
    public class VoucherQueueWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VoucherQueueWorker> _logger;

        public VoucherQueueWorker(IServiceProvider serviceProvider, ILogger<VoucherQueueWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Voucher Sync Worker is starting (Cycle: 10 days).");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<EngagementDbContext>();

                        // 1. Thực hiện đồng bộ trong một Transaction lớn
                        using (var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken))
                        {
                            try
                            {
                                _logger.LogInformation("Starting batch update from VoucherQueue...");

                                // 2. Cập nhật CurrentQuantity bằng cách trừ đi số lượng đếm được trong Queue
                                // Sử dụng SQL để tối ưu hiệu suất (tránh kéo hàng ngàn record lên RAM)
                                var updateSql = @"
                                    UPDATE vouchers v
                                    SET current_quantity = v.current_quantity - q.total_redeemed
                                    FROM (
                                        SELECT voucher_id, COUNT(*) as total_redeemed
                                        FROM voucher_queue
                                        GROUP BY voucher_id
                                    ) q
                                    WHERE v.id = q.voucher_id";

                                int updatedRows = await dbContext.Database.ExecuteSqlRawAsync(updateSql, stoppingToken);

                                // 3. Xóa sạch bảng Queue sau khi đã cập nhật xong
                                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM voucher_queue", stoppingToken);

                                await transaction.CommitAsync(stoppingToken);
                                _logger.LogInformation($"Successfully synced {updatedRows} vouchers and cleared queue.");
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync(stoppingToken);
                                _logger.LogError($"Critical error during batch sync: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Worker error: {ex.Message}");
                }

                // 4. Nghỉ 10 ngày
                _logger.LogInformation("Worker sleeping for 10 days...");
                await Task.Delay(TimeSpan.FromDays(14), stoppingToken);
            }
        }
    }
}
