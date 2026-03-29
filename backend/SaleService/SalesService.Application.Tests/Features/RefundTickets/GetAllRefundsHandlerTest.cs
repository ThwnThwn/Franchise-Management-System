using Moq;
using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Queries.GetAll;
using SalesService.Application.Persisitences;
using SalesService.Application.Interfaces.Identity;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class GetAllRefundsHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _ticketRepositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityServiceClientMock;
    private readonly GetAllRefundsHandler _handler;

    public GetAllRefundsHandlerTests()
    {
        _ticketRepositoryMock = new Mock<IRefundTicketRepository>();
        _identityServiceClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetAllRefundsHandler(_ticketRepositoryMock.Object, _identityServiceClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedRefunds_WhenDataExists()
    {
        // Arrange
        var req = new GetAllRefundsQueryRequest { UserId = Guid.NewGuid(), Status = RefundTicketStatus.Pending };
        var query = new GetAllRefundsQuery(req, 1, 10);

        var refunds = new List<RefundTicket>
        {
            new RefundTicket { Id = Guid.NewGuid(), TicketCode = "REF1", Amount = 100, Reason = "Defective", Status = RefundTicketStatus.Pending },
            new RefundTicket { Id = Guid.NewGuid(), TicketCode = "REF2", Amount = 200, Reason = "Changed mind", Status = RefundTicketStatus.Pending }
        };

        var pagedData = PagedResult<RefundTicket>.Create(refunds, 2, 1, 10);

        _ticketRepositoryMock.Setup(x => x.GetAllRefundsAsync(
                query.Request.UserId, query.Request.Status, query.PageIndex, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.First().TicketCode.Should().Be("REF1");
        result.Value.Items.Last().TicketCode.Should().Be("REF2");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenNoDataExists()
    {
        // Arrange
        var req = new GetAllRefundsQueryRequest();
        var query = new GetAllRefundsQuery(req, 1, 10);

        _ticketRepositoryMock.Setup(x => x.GetAllRefundsAsync(
                null, null, query.PageIndex, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<RefundTicket>?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }
}
