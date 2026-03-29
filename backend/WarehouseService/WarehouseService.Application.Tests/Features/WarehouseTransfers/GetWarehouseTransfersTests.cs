namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentAssertions;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.WarehouseTransfers.Query.GetAll;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class GetWarehouseTransfersTests
{
    private readonly Mock<IWarehouseTransferRepository> _warehouseTransferRepositoryMock;
    private readonly GetWarehouseTransfersQueryHandler _handler;

    public GetWarehouseTransfersTests()
    {
        _warehouseTransferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _handler = new GetWarehouseTransfersQueryHandler(_warehouseTransferRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTransfers_WhenNoFiltersApplied()
    {
        var transfers = new List<WarehouseTransfer>
        {
            new() { Id = Guid.NewGuid(), TransferCode = "TRF-ARABICA-01", CreatedAt = DateTimeOffset.UtcNow, TransferItems = [] },
            new() { Id = Guid.NewGuid(), TransferCode = "TRF-ROBUSTA-02", CreatedAt = DateTimeOffset.UtcNow, TransferItems = [] }
        }.BuildMock();

        _warehouseTransferRepositoryMock.Setup(x => x.GetQueryable()).Returns(transfers);

        var query = new GetWarehouseTransfersQuery(null, null, null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByWarehouse_WhenFromOrToIdProvided()
    {
        var fromWhId = Guid.NewGuid();
        var toWhId = Guid.NewGuid();
        var transfers = new List<WarehouseTransfer>
        {
            new() { FromWarehouseId = fromWhId, ToWarehouseId = Guid.NewGuid(), TransferCode = "TRF-FROM", TransferItems = [] },
            new() { FromWarehouseId = Guid.NewGuid(), ToWarehouseId = toWhId, TransferCode = "TRF-TO", TransferItems = [] }
        }.BuildMock();

        _warehouseTransferRepositoryMock.Setup(x => x.GetQueryable()).Returns(transfers);

        // Test FromWarehouseId
        var queryFrom = new GetWarehouseTransfersQuery(fromWhId, null, null, null, null);
        var resultFrom = await _handler.Handle(queryFrom, CancellationToken.None);

        // Test ToWarehouseId
        var queryTo = new GetWarehouseTransfersQuery(null, toWhId, null, null, null);
        var resultTo = await _handler.Handle(queryTo, CancellationToken.None);

        resultFrom.Items.Should().HaveCount(1);
        resultFrom.Items[0].TransferCode.Should().Be("TRF-FROM");
        resultTo.Items.Should().HaveCount(1);
        resultTo.Items[0].TransferCode.Should().Be("TRF-TO");
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenStatusIsProvided()
    {
        var transfers = new List<WarehouseTransfer>
        {
            new() { Status = TransferStatus.Pending, TransferCode = "PENDING-01", TransferItems = [] },
            new() { Status = TransferStatus.Approved, TransferCode = "APPROVED-01", TransferItems = [] }
        }.BuildMock();

        _warehouseTransferRepositoryMock.Setup(x => x.GetQueryable()).Returns(transfers);

        var query = new GetWarehouseTransfersQuery(null, null, TransferStatus.Pending, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(TransferStatus.Pending);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateRange_WhenDatesAreProvided()
    {
        var fromDate = DateTimeOffset.UtcNow.AddDays(-5);
        var toDate = DateTimeOffset.UtcNow.AddDays(-2);

        var transfers = new List<WarehouseTransfer>
        {
            new() { CreatedAt = fromDate.AddDays(1), TransferCode = "IN-RANGE", TransferItems = [] },
            new() { CreatedAt = DateTimeOffset.UtcNow, TransferCode = "OUT-RANGE", TransferItems = [] }
        }.BuildMock();

        _warehouseTransferRepositoryMock.Setup(x => x.GetQueryable()).Returns(transfers);

        var query = new GetWarehouseTransfersQuery(null, null, null, fromDate, toDate);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].TransferCode.Should().Be("IN-RANGE");
    }

    [Fact]
    public async Task Handle_ShouldApplyPaginationCorrectly()
    {
        var transfers = Enumerable.Range(1, 15).Select(i => new WarehouseTransfer
        {
            Id = Guid.NewGuid(),
            TransferCode = $"TRF-COFFEE-{i:00}",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(i),
            TransferItems = []
        }).ToList().BuildMock();

        _warehouseTransferRepositoryMock.Setup(x => x.GetQueryable()).Returns(transfers);

        var query = new GetWarehouseTransfersQuery(null, null, null, null, null, Page: 2, PageSize: 5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(2);
    }
}