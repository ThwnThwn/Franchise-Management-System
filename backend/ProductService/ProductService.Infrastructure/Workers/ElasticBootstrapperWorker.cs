using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using SharedKernel.Events;

namespace ProductService.Infrastructure.Workers;

public class ElasticBootstrapperWorker(
    IServiceScopeFactory scopeFactory,
    IDistributedLock distributedLock,
    ILogger<ElasticBootstrapperWorker> logger) : BackgroundService
{
    private const string IndexName = "products";
    private const string LockKey = "elastic_bootstrapper_lock";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Đợi một chút để các Service khác (DB, Elastic) khởi động xong hoàn toàn
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        logger.LogInformation("Smart Bootstrapper: Checking data status...");

        // Dùng 'await using' để tự động gọi DisposeAsync (nhả lock) khi kết thúc block
        await using var lockHandle = await distributedLock.TryAcquireLockAsync(LockKey, TimeSpan.FromMinutes(30));

        // 1. NGĂN ĐỤNG ĐỘ: Chỉ 1 instance được phép chạy Sync tại một thời điểm
        if (lockHandle is not null)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var elasticClient = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();
                var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var categoryRepo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();

                // 1. NUKE: Xóa sạch Index cũ để reset hoàn toàn
                logger.LogWarning("Resetting Elasticsearch index: Deleting '{IndexName}'...", IndexName);

                // Xóa index (nếu tồn tại)
                var deleteResponse = await elasticClient.Indices.DeleteAsync(IndexName, stoppingToken);

                if (deleteResponse.IsSuccess() || deleteResponse.ElasticsearchServerError?.Status == 404)
                {
                    logger.LogInformation("Index '{IndexName}' deleted successfully or did not exist.", IndexName);
                }
                else
                {
                    logger.LogError("Failed to delete index '{IndexName}': {Error}", IndexName, deleteResponse.DebugInformation);
                }

                // 2. SYNC: Bắt đầu nạp lại dữ liệu từ Postgres
                logger.LogInformation("Starting full data synchronization from Postgres to Elasticsearch...");

                await PerformBulkSync(productRepo, categoryRepo, elasticClient, stoppingToken);

                logger.LogInformation("Full synchronization completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error during Elasticsearch full reset and bootstrapping.");
            }
        }
        else
        {
            logger.LogInformation("Another instance is holding the lock. Skipping bootstrapping.");
        }
    }

    private async Task PerformBulkSync(
        IProductRepository productRepo,
        ICategoryRepository categoryRepo,
        ElasticsearchClient client,
        CancellationToken ct)
    {
        int pageIndex = 1;
        int pageSize = 100; // Mỗi lần bốc 100 bản ghi để không treo RAM
        int totalProcessed = 0;

        while (!ct.IsCancellationRequested)
        {
            // Lấy dữ liệu từ Postgres (Dùng hàm GetAll có phân trang của bro)
            var pagedProducts = await productRepo.GetAllProductWithPagingByAsync(null, null, pageIndex, pageSize, ct);

            if (!pagedProducts.Items.Any()) break;

            var bulkDocuments = new List<ProductPersistedIntegrationEvent>();

            foreach (var p in pagedProducts.Items)
            {
                // Chuyển đổi sang Integration Model (Bro nên để logic này vào 1 Mapper hoặc Service riêng)
                var doc = new ProductPersistedIntegrationEvent(
                    p.Id, p.Name, p.Description, p.ImageUrl, p.ImagePublicId,
                    p.IsActive ?? true, p.IsDeleted, p.CreatedAt, p.UpdatedAt,
                    p.CategoryId, p.Category?.Name ?? "N/A",
                    p.ProductVariants.Select(v => new ProductVariantIntegrationModel(
                        v.Id, v.Sku, v.Name, v.Attributes, v.BasePrice, v.Cost, v.IsActive ?? true
                    )).ToList()
                );
                bulkDocuments.Add(doc);
            }

            // 4. BULK INSERT: Đẩy 100 bản ghi lên Elastic trong 1 request duy nhất
            var bulkResponse = await client.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(bulkDocuments, (desc, doc) => desc.Id(doc.Id.ToString()))
            , ct);

            if (!bulkResponse.IsSuccess())
            {
                logger.LogError("Bulk Sync error at page {Page}: {Error}", pageIndex, bulkResponse.DebugInformation);
            }

            totalProcessed += bulkDocuments.Count;
            logger.LogInformation("Successfully synchronized {Total} products...", totalProcessed);

            pageIndex++;
        }

        logger.LogInformation("Smart Bootstrapping completed. Total: {Total} products.", totalProcessed);
    }
}