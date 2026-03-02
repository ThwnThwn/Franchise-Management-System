using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetLoyaltyReport
{
    public record GetLoyaltyReportQuery(
        DateTime? FromDate,
        DateTime? ToDate,
        string? TierId,
        ExportFormat Format
    ) : IRequest<LoyaltyReportResponse>;

    public record LoyaltyReportResponse(
        byte[]? FileContent,
        string? ContentType,
        string? FileName,
        LoyaltyReportDto? Data);
}
