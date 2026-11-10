using Moq;
using FluentAssertions;
using SalesService.Application.Features.WalletTransactions.Querries.GetByOrderId;
using SalesService.Application.Features.WalletTransactions.Queries.GetById;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class GetWalletTransactionByOrderIdQueryHandlerTests
{
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly GetWalletTransactionByOrderIdQueryHandler _handler;

    public GetWalletTransactionByOrderIdQueryHandlerTests()
    {
        _handler = new GetWalletTransactionByOrderIdQueryHandler(_txnRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenNoTransactions()
    {
        var query = new GetWalletTransactionByOrderIdQuery(Guid.NewGuid(), Guid.NewGuid());
        _txnRepoMock.Setup(x => x.GetByOrderIdAsync(query.OrderId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<WalletTransaction>());
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenNotOwnerAndNotInternal()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var query = new GetWalletTransactionByOrderIdQuery(Guid.NewGuid(), userId);

        var txns = new List<WalletTransaction>
        {
            new() { Id = Guid.NewGuid(), UserId = otherUserId, TransactionCode = "T1", Amount = 100, TransactionType = TransactionType.Payment }
        };

        _txnRepoMock.Setup(x => x.GetByOrderIdAsync(query.OrderId, It.IsAny<CancellationToken>())).ReturnsAsync(txns);
        _identityMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOwner()
    {
        var userId = Guid.NewGuid();
        var query = new GetWalletTransactionByOrderIdQuery(Guid.NewGuid(), userId);

        var txns = new List<WalletTransaction>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, TransactionCode = "T1", Amount = 100, TransactionType = TransactionType.Payment }
        };

        _txnRepoMock.Setup(x => x.GetByOrderIdAsync(query.OrderId, It.IsAny<CancellationToken>())).ReturnsAsync(txns);
        _identityMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}

public class GetWalletTransactionByIdQueryHandlerTests
{
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly GetWalletTransactionByIdQueryHandler _handler;

    public GetWalletTransactionByIdQueryHandlerTests()
    {
        _handler = new GetWalletTransactionByIdQueryHandler(_txnRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTransactionNotExists()
    {
        var query = new GetWalletTransactionByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        _identityMock.Setup(x => x.GetUserByIdAsync(query.LoggedUserId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "admin" } });
        _txnRepoMock.Setup(x => x.GetByIdAsync(query.TransactionId, It.IsAny<CancellationToken>())).ReturnsAsync((WalletTransaction?)null);

        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOwner()
    {
        var userId = Guid.NewGuid();
        var txnId = Guid.NewGuid();
        var query = new GetWalletTransactionByIdQuery(txnId, userId);
        var txn = new WalletTransaction { Id = txnId, UserId = userId, TransactionCode = "T1", Amount = 100, TransactionType = TransactionType.Deposit };

        _identityMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });
        _txnRepoMock.Setup(x => x.GetByIdAsync(txnId, It.IsAny<CancellationToken>())).ReturnsAsync(txn);

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(txnId);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenNotOwnerAndNotInternal()
    {
        var userId = Guid.NewGuid();
        var txnId = Guid.NewGuid();
        var query = new GetWalletTransactionByIdQuery(txnId, userId);
        var txn = new WalletTransaction { Id = txnId, UserId = Guid.NewGuid(), TransactionCode = "T1", Amount = 100, TransactionType = TransactionType.Deposit };

        _identityMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "customer" } });
        _txnRepoMock.Setup(x => x.GetByIdAsync(txnId, It.IsAny<CancellationToken>())).ReturnsAsync(txn);

        var result = await _handler.Handle(query, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }
}
