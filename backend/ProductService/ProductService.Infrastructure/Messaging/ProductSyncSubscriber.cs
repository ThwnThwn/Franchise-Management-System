using DotNetCore.CAP;
using Elastic.Clients.Elasticsearch;
using SharedKernel.Events;
using Microsoft.Extensions.Logging;

namespace ProductService.Infrastructure.Messaging;

public class ProductSyncSubscriber(
    ElasticsearchClient elasticClient,
    ILogger<ProductSyncSubscriber> logger) : ICapSubscribe
{
    private const string IndexName = "products";

    /// <summary>
    /// 5.2. Lắng nghe Create và Update (Gom chung vì dùng IndexRequest là Upsert)
    /// </summary>
    [CapSubscribe("product.persisted")]
    [CapSubscribe("product.created")]
    [CapSubscribe("product.updated")]
    public async Task HandleProductPersisted(ProductPersistedIntegrationEvent @event)
    {
        logger.LogInformation("ElasticSync: Đang Index dữ liệu cho SP {Id}", @event.Id);

        // Lưu ý: Elasticsearch v8 sẽ tự động chuyển string sang Id object
        var request = new IndexRequest<ProductPersistedIntegrationEvent>(
            @event,
            index: IndexName,
            id: @event.Id.ToString()
        );

        var response = await elasticClient.IndexAsync(request);

        if (!response.IsSuccess())
        {
            logger.LogError("ElasticSync Index Fail: {Error}", response.DebugInformation);
        }
    }

    /// <summary>
    /// 5.2. Lắng nghe Delete
    /// </summary>
    [CapSubscribe("product.deleted")]
    public async Task HandleProductDeleted(ProductDeletedIntegrationEvent @event)
    {
        logger.LogInformation("ElasticSync: Đang xóa Document SP {Id}", @event.Id);

        // Khởi tạo DeleteRequest với Index và Id tường minh
        var request = new DeleteRequest(IndexName, @event.Id.ToString());

        var response = await elasticClient.DeleteAsync(request);

        if (!response.IsSuccess() && response.ElasticsearchServerError?.Status != 404)
        {
            logger.LogError("ElasticSync Delete Fail: {Error}", response.DebugInformation);
        }
    }
}