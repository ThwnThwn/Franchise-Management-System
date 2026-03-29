using Moq;
using FluentAssertions;
using SalesService.Application.Features.WalletTransactions.Queries.GetAll;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class GetAllWalletTransactionsQueryHandlerTests
{
    private readonly Mock<IWalletTransactionRepository> _walletTransactionRepositoryMock;
    private readonly GetAllWalletTransactionsQueryHandler _handler;

    public GetAllWalletTransactionsQueryHandlerTests()
    {
        _walletTransactionRepositoryMock = new Mock<IWalletTransactionRepository>();
        _handler = new GetAllWalletTransactionsQueryHandler(_walletTransactionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedTransactions_WhenDataExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllWalletTransactionsQuery(userId, 1, 10);

        var transactions = new List<WalletTransaction>
        {
            new WalletTransaction { Id = Guid.NewGuid(), TransactionCode = "TXN1", Amount = 1500, Status = TransactionStatus.Completed },
            new WalletTransaction { Id = Guid.NewGuid(), TransactionCode = "TXN2", Amount = -500, Status = TransactionStatus.Completed }
        };

        var pagedData = PagedResult<WalletTransaction>.Create(transactions, 2, 1, 10);

        _walletTransactionRepositoryMock.Setup(x => x.GetAllTransactionsAsync(
                query.PageIndex, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.First().TransactionCode.Should().Be("TXN1");
        result.Value.Items.Last().TransactionCode.Should().Be("TXN2");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenNoDataExists()
    {
        // Arrange
        var query = new GetAllWalletTransactionsQuery(Guid.NewGuid(), 1, 10);

        _walletTransactionRepositoryMock.Setup(x => x.GetAllTransactionsAsync(
                query.PageIndex, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<WalletTransaction>?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }
}
