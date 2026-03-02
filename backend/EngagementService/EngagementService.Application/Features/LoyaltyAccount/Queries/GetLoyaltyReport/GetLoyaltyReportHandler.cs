using EngagementService.Application.Abstraction;
using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetLoyaltyReport
{
    public class GetLoyaltyReportHandler : IRequestHandler<GetLoyaltyReportQuery, LoyaltyReportResponse>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IReportService _reportService;

        public GetLoyaltyReportHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IReportService reportService)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _reportService = reportService;
        }
        public async Task<LoyaltyReportResponse> Handle(GetLoyaltyReportQuery request, CancellationToken ct)
        {
            // 1. Lấy dữ liệu từ Repository (đã bao gồm lọc theo TierId và Date)
            var reportData = await _loyaltyAccountRepository.GetLoyaltyReportDataAsync(
                request.FromDate,
                request.ToDate,
                request.TierId,
                ct);

            // 2. Sử dụng Switch Expression trên Enum để xử lý Export
            return request.Format switch
            {
                ExportFormat.Pdf => new LoyaltyReportResponse(
                    _reportService.ExportToPdf(reportData),
                    "application/pdf",
                    $"LoyaltyReport_{DateTime.Now:yyyyMMdd}.pdf",
                    reportData),

                ExportFormat.Csv => new LoyaltyReportResponse(
                    _reportService.ExportToCsv(reportData),
                    "text/csv",
                    $"LoyaltyReport_{DateTime.Now:yyyyMMdd}.csv",
                    reportData),

                ExportFormat.Excel => throw new NotImplementedException("Chức năng xuất Excel đang được phát triển."),

                // Mặc định là ExportFormat.View: Trả về dữ liệu JSON để hiển thị Dashboard
                _ => new LoyaltyReportResponse(null, null, null, reportData)
            };
        }
    }
}
