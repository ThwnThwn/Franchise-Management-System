using BuildingBlocks.Core.Pagination;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ProductService.Application.Dtos;
using ProductService.Application.SearchEngines;

namespace ProductService.Infrastructure.SearchEngines;

public class ElasticProductSearchEngine(ElasticsearchClient client) : IProductSearchEngine
{
    private const string IndexName = "products";
    private static readonly string[] fields = ["name^3", "categoryName^2", "description", "variants.sku"];

    public async Task<PagedResult<ProductDto>> GetAllProductWithPagingByAsync(
        string? keyword,
        Guid? categoryId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Tạo một list chứa các Action (điều kiện lọc)
        var filters = new List<Action<QueryDescriptor<ProductDto>>>
        {
            // 2. Add điều kiện mặc định vào list
            f => f.Term(t => t.Field(p => p.IsDeleted).Value(false))
        };

        // 3. Nếu có CategoryId, add tiếp vào list
        if (categoryId.HasValue && categoryId != Guid.Empty)
        {
            filters.Add(f => f.Term(t => t.Field("categoryId.keyword").Value(categoryId.Value.ToString().ToLower())));
        }

        var response = await client.SearchAsync<ProductDto>(s => s
            .Index(IndexName)
            .From((pageIndex - 1) * pageSize)
            .Size(pageSize)
            .TrackTotalHits(new TrackHits(true))
            .Query(q => q
                .Bool(b => b
                    .Filter(filters.ToArray())
                    .Must(m =>
                    {
                        if (!string.IsNullOrWhiteSpace(keyword))
                        {
                            m.MultiMatch(mm => mm
                                .Query(keyword)
                                .Fields(fields)
                                .Fuzziness(new Fuzziness("AUTO"))
                            );
                        }
                        else
                        {
                            m.MatchAll(ma => ma.Boost(1.0f));
                        }
                    })
                )
            )
            .Sort(srt => srt.Field(p => p.CreatedAt, f => f.Order(SortOrder.Desc))),
        cancellationToken);

        if (!response.IsSuccess())
        {
            // 1. Lỗi do ES trả về (VD: sai cú pháp query, index không tồn tại)
            if (response.ElasticsearchServerError != null)
            {
                var error = response.ElasticsearchServerError.Error;
                Console.WriteLine($"[ES Error] Type: {error.Type}");
                Console.WriteLine($"[ES Error] Reason: {error.Reason}");
            }

            // 2. Lỗi mạng (VD: rớt mạng, sập server ES, timeout)
            if (response.ApiCallDetails?.OriginalException != null)
            {
                Console.WriteLine($"[ES Exception]: {response.ApiCallDetails.OriginalException.Message}");
            }

            // 3. QUAN TRỌNG NHẤT: In ra toàn bộ Debug Info
            // Thằng này sẽ in ra cả HTTP Status, URL, và ĐẶC BIỆT LÀ CÁI JSON REQUEST
            // Bro copy cái JSON này ném vào Kibana Dev Tools chạy thử là ra ngay bệnh!
            Console.WriteLine($"[ES Debug Info]:\n{response.DebugInformation}");

            return new PagedResult<ProductDto>([], 0, pageIndex, pageSize);
        }

        var totalCount = response.HitsMetadata.Total?.Match(
            totalHits => totalHits.Value, // Nếu ES trả về object {"value": 100, "relation": "eq"}
            longValue => longValue        // Nếu ES trả về con số đơn thuần
        ) ?? 0;

        return new PagedResult<ProductDto>(
            response.Documents.ToList(),
            (int)totalCount,
            pageIndex,
            pageSize
        );
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = new GetRequest(IndexName, id.ToString());

        var response = await client.GetAsync<ProductDto>(request, cancellationToken);

        return response.IsSuccess() ? response.Source : null;
    }
}