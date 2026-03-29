using Moq;
using FluentAssertions;
using SalesService.Application.Features.WalletTransactions.Queries.GetByUserId;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class GetWalletTransactionsByUserIdQueryHandlerTests
{
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly GetWalletTransactionsByUserIdQueryHandler _handler;

    public GetWalletTransactionsByUserIdQueryHandlerTests()
    {
        _handler = new GetWalletTransactionsByUserIdQueryHandler(_txnRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenNoTransactions()
    {
        var query = new GetWalletTransactionsByUserIdQuery(Guid.NewGuid(), Guid.NewGuid(), 1, 10);
        _txnRepoMock.Setup(x => x.GetByUserIdAsync(query.UserIdToSearch, query.PageIndex, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<WalletTransaction>?)null);

        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenNotOwnerAndNotInternal()
    {
        var userIdToSearch = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();
        var query = new GetWalletTransactionsByUserIdQuery(userIdToSearch, loggedUserId, 1, 10);

        var paged = PagedResult<WalletTransaction>.Create(new List<WalletTransaction> { new() { Id = Guid.NewGuid() } }, 1, 1, 10);
        _txnRepoMock.Setup(x => x.GetByUserIdAsync(userIdToSearch, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
        _identityMock.Setup(x => x.GetUserByIdAsync(loggedUserId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOwner()
    {
        var userId = Guid.NewGuid();
        var query = new GetWalletTransactionsByUserIdQuery(userId, userId, 1, 10);

        var paged = PagedResult<WalletTransaction>.Create(new List<WalletTransaction> { new() { Id = Guid.NewGuid(), TransactionCode = "T1" } }, 1, 1, 10);
        _txnRepoMock.Setup(x => x.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
        _identityMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenInternalUser()
    {
        var userIdToSearch = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();
        var query = new GetWalletTransactionsByUserIdQuery(userIdToSearch, loggedUserId, 1, 10);

        var paged = PagedResult<WalletTransaction>.Create(new List<WalletTransaction> { new() { Id = Guid.NewGuid(), TransactionCode = "T1" } }, 1, 1, 10);
        _txnRepoMock.Setup(x => x.GetByUserIdAsync(userIdToSearch, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(paged);
        _identityMock.Setup(x => x.GetUserByIdAsync(loggedUserId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "admin" } });

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
    }
}
