using Moq;
using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Queries.GetByStatus;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class GetRefundByStatusQueryHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _refundRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly GetRefundByStatusQueryHandler _handler;

    public GetRefundByStatusQueryHandlerTests()
    {
        _handler = new GetRefundByStatusQueryHandler(_refundRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenPageIndexLessThan1()
    {
        var query = new GetRefundByStatusQuery(null, RefundTicketStatus.Pending, 0, 10);
        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenPageSizeTooLarge()
    {
        var query = new GetRefundByStatusQuery(null, RefundTicketStatus.Pending, 1, 200);
        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("between 1 and 100"));
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenAdminViewsAll()
    {
        var query = new GetRefundByStatusQuery(null, RefundTicketStatus.Pending, 1, 10);
        var tickets = new List<RefundTicket>
        {
            new() { Id = Guid.NewGuid(), TicketCode = "T1", UserId = Guid.NewGuid(), Amount = 100, Status = RefundTicketStatus.Pending },
            new() { Id = Guid.NewGuid(), TicketCode = "T2", UserId = Guid.NewGuid(), Amount = 200, Status = RefundTicketStatus.Pending }
        };
        var paged = PagedResult<RefundTicket>.Create(tickets, 2, 1, 10);
        _refundRepoMock.Setup(x => x.GetByStatusAsync(RefundTicketStatus.Pending, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(paged);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenUserViewsOwn()
    {
        var userId = Guid.NewGuid();
        var query = new GetRefundByStatusQuery(userId, RefundTicketStatus.Pending, 1, 10);
        var tickets = new List<RefundTicket>
        {
            new() { Id = Guid.NewGuid(), TicketCode = "T1", UserId = userId, Amount = 100, Status = RefundTicketStatus.Pending }
        };
        var paged = PagedResult<RefundTicket>.Create(tickets, 1, 1, 10);
        _refundRepoMock.Setup(x => x.GetByUserIdAndStatusAsync(userId, RefundTicketStatus.Pending, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(paged);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
    }
}
