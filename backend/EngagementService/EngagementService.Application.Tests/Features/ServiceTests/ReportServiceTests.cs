using EngagementService.Application.Dtos.LoyaltyAccount;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;
using System.Text;

namespace EngagementService.Application.Tests;

public class ReportServiceTests
{
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        _reportService = new ReportService();
    }

    private LoyaltyReportDto GetMockReportData()
    {
        return new LoyaltyReportDto
        {
            TotalMembers = 100,
            TotalPointsEarned = 5000,
            TotalPointsRedeemed = 2000,
            TierDistribution = new List<TierDistributionDto>
            {
                new TierDistributionDto { TierName = "Vàng", MemberCount = 30 },
                new TierDistributionDto { TierName = "Bạc", MemberCount = 70 }
            }
        };
    }

    // 1. Phủ xanh ExportToCsv (Dòng 13-58)
    [Fact]
    public void ExportToCsv_ShouldReturnValidByteArray()
    {
        // Arrange
        var data = GetMockReportData();

        // Act
        var result = _reportService.ExportToCsv(data);
        var csvContent = Encoding.UTF8.GetString(result);

        // Assert
        result.Should().NotBeEmpty();
        csvContent.Should().Contain("BÁO CÁO CHƯƠNG TRÌNH KHÁCH HÀNG THÂN THIẾT");
        csvContent.Should().Contain("Tổng thành viên");
        csvContent.Should().Contain("100"); // Kiểm tra dữ liệu mock
        csvContent.Should().Contain("Vàng");
    }

    // 2. Phủ xanh ExportToPdf và SummaryCard (Dòng 61-155)
    [Fact]
    public void ExportToPdf_ShouldReturnValidPdfByteArray()
    {
        // Arrange
        var data = GetMockReportData();

        // Act
        var result = _reportService.ExportToPdf(data);

        // Assert
        result.Should().NotBeEmpty();

        // Kiểm tra chữ ký file PDF (PDF header luôn bắt đầu bằng %PDF)
        var pdfString = Encoding.ASCII.GetString(result, 0, 4);
        pdfString.Should().Be("%PDF");
    }
}