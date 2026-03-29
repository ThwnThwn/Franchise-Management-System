using Moq;
using FluentAssertions;
using SalesService.Application.Features.Wallets.Queries.GetByUserId;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Wallets;

public class GetWalletByUserIdQueryHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly GetWalletByUserIdQueryHandler _handler;

    public GetWalletByUserIdQueryHandlerTests()
    {
        _handler = new GetWalletByUserIdQueryHandler(_walletRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWalletDoesNotExist()
    {
        var query = new GetWalletByUserIdQuery(Guid.NewGuid());
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(query.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenWalletExists()
    {
        var userId = Guid.NewGuid();
        var query = new GetWalletByUserIdQuery(userId);
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 500, Status = WalletStatus.Active };
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(500);
        result.Value.UserId.Should().Be(userId);
    }
}
