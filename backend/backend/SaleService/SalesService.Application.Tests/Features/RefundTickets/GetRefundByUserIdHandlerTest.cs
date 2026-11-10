using Moq;
using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Queries.GetByUserId;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Domain.Entities;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class GetRefundByUserIdHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _refundRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly GetRefundByUserIdHandler _handler;

    public GetRefundByUserIdHandlerTests()
    {
        _handler = new GetRefundByUserIdHandler(_refundRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenNotAdminAndNotOwner()
    {
        var userIdToSearch = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();
        var query = new GetRefundByUserIdQuery(userIdToSearch, loggedUserId, 1, 10);

        var tickets = PagedResult<RefundTicket>.Create(new List<RefundTicket>
        {
            new() { Id = Guid.NewGuid(), UserId = userIdToSearch, TicketCode = "T1" }
        }, 1, 1, 10);

        _refundRepoMock.Setup(x => x.GetByUserIdAsync(userIdToSearch, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(tickets);
        _identityMock.Setup(x => x.GetUserByIdAsync(loggedUserId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOwner()
    {
        var userId = Guid.NewGuid();
        var query = new GetRefundByUserIdQuery(userId, userId, 1, 10);

        var tickets = PagedResult<RefundTicket>.Create(new List<RefundTicket>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, TicketCode = "T1" }
        }, 1, 1, 10);

        _refundRepoMock.Setup(x => x.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(tickets);
        _identityMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAdmin()
    {
        var userIdToSearch = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();
        var query = new GetRefundByUserIdQuery(userIdToSearch, loggedUserId, 1, 10);

        var tickets = PagedResult<RefundTicket>.Create(new List<RefundTicket>
        {
            new() { Id = Guid.NewGuid(), UserId = userIdToSearch, TicketCode = "T1" }
        }, 1, 1, 10);

        _refundRepoMock.Setup(x => x.GetByUserIdAsync(userIdToSearch, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(tickets);
        _identityMock.Setup(x => x.GetUserByIdAsync(loggedUserId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "admin" } });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
    }
}
