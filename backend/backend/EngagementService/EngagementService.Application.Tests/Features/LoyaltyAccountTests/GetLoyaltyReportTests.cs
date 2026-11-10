using EngagementService.Application.Abstraction;
using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Features.LoyaltyAccount.Queries.GetLoyaltyReport;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class GetLoyaltyReportHandlerTests
{
    private readonly Mock<ILoyaltyAccountRepository> _repoMock;
    private readonly Mock<IReportService> _reportServiceMock;
    private readonly GetLoyaltyReportHandler _handler;

    public GetLoyaltyReportHandlerTests()
    {
        _repoMock = new Mock<ILoyaltyAccountRepository>();
        _reportServiceMock = new Mock<IReportService>();
        _handler = new GetLoyaltyReportHandler(_repoMock.Object, _reportServiceMock.Object);
    }

    // 1. Phủ nhánh: Export PDF (Dòng 29-33)
    [Fact]
    public async Task Handle_ShouldReturnPdfResponse_WhenFormatIsPdf()
    {
        // Arrange
        var query = new GetLoyaltyReportQuery(null, null, null, ExportFormat.Pdf);
        var reportData = new LoyaltyReportDto();
        var fileBytes = new byte[] { 1, 2, 3 };

        _repoMock.Setup(x => x.GetLoyaltyReportDataAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);
        _reportServiceMock.Setup(x => x.ExportToPdf(reportData)).Returns(fileBytes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentType.Should().Be("application/pdf");
        result.FileContent.Should().BeEquivalentTo(fileBytes);
        result.FileName.Should().Contain(".pdf");
    }

    // 2. Phủ nhánh: Export CSV (Dòng 35-39)
    [Fact]
    public async Task Handle_ShouldReturnCsvResponse_WhenFormatIsCsv()
    {
        // Arrange
        var query = new GetLoyaltyReportQuery(null, null, null, ExportFormat.Csv);
        var reportData = new LoyaltyReportDto();
        var fileBytes = new byte[] { 4, 5, 6 };

        _repoMock.Setup(x => x.GetLoyaltyReportDataAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);
        _reportServiceMock.Setup(x => x.ExportToCsv(reportData)).Returns(fileBytes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentType.Should().Be("text/csv");
        result.FileContent.Should().BeEquivalentTo(fileBytes);
        result.FileName.Should().Contain(".csv");
    }

    // 3. Phủ nhánh: Excel NotImplemented (Dòng 41)
    [Fact]
    public async Task Handle_ShouldThrowNotImplementedException_WhenFormatIsExcel()
    {
        // Arrange
        var query = new GetLoyaltyReportQuery(null, null, null, ExportFormat.Excel);
        _repoMock.Setup(x => x.GetLoyaltyReportDataAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoyaltyReportDto());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("Chức năng xuất Excel đang được phát triển.");
    }

    // 4. Phủ nhánh: Mặc định (View Dashboard) (Dòng 44)
    [Fact]
    public async Task Handle_ShouldReturnDataOnly_WhenFormatIsDefaultOrView()
    {
        // Arrange
        // Giả sử ExportFormat có giá trị View hoặc một giá trị không nằm trong 3 cái trên
        var query = new GetLoyaltyReportQuery(null, null, null, (ExportFormat)999);
        var reportData = new LoyaltyReportDto();

        _repoMock.Setup(x => x.GetLoyaltyReportDataAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.FileContent.Should().BeNull();
        result.Data.Should().Be(reportData);
    }
}