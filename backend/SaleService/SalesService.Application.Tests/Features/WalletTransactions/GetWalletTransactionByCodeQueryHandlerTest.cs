using Moq;
using FluentAssertions;
using SalesService.Application.Features.WalletTransactions.Queries.GetByCode;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class GetWalletTransactionByCodeQueryHandlerTests
{
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly GetWalletTransactionByCodeQueryHandler _handler;

    public GetWalletTransactionByCodeQueryHandlerTests()
    {
        _handler = new GetWalletTransactionByCodeQueryHandler(_txnRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTransactionNotFound()
    {
        var query = new GetWalletTransactionByCodeQuery("T1", Guid.NewGuid());
        _txnRepoMock.Setup(x => x.GetByTransactionCodeAsync(query.TransactionCode, It.IsAny<CancellationToken>())).ReturnsAsync((WalletTransaction?)null);

        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenNotOwnerAndNotInternal()
    {
        var query = new GetWalletTransactionByCodeQuery("T1", Guid.NewGuid());
        var txn = new WalletTransaction { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TransactionCode = "T1" };

        _txnRepoMock.Setup(x => x.GetByTransactionCodeAsync(query.TransactionCode, It.IsAny<CancellationToken>())).ReturnsAsync(txn);
        _identityMock.Setup(x => x.GetUserByIdAsync(query.LoggedUserId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOwner()
    {
        var userId = Guid.NewGuid();
        var query = new GetWalletTransactionByCodeQuery("T1", userId);
        var txn = new WalletTransaction { Id = Guid.NewGuid(), UserId = userId, TransactionCode = "T1" };

        _txnRepoMock.Setup(x => x.GetByTransactionCodeAsync(query.TransactionCode, It.IsAny<CancellationToken>())).ReturnsAsync(txn);
        _identityMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCode.Should().Be("T1");
    }
}
