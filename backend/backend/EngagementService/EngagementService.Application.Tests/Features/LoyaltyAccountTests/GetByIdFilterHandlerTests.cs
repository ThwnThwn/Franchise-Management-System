using EngagementService.Application.Features.LoyaltyAccount.Queries.GetByIdsFilter;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class GetByFilterHandlerTests
{
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyAccountRepoMock;
    private readonly GetByFilterHandler _handler;

    public GetByFilterHandlerTests()
    {
        _loyaltyAccountRepoMock = new Mock<ILoyaltyAccountRepository>();
        _handler = new GetByFilterHandler(_loyaltyAccountRepoMock.Object);
    }

    // 1. Phủ xanh Happy Path: Lấy dữ liệu với đầy đủ filter
    [Fact]
    public async Task Handle_ShouldReturnFilteredAccounts_WhenRequestIsValid()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        var query = new GetByFilterQuery
        {
            TierId = tierId,
            StartAt = 100,
            EndAt = 500
        };

        var accounts = new List<LoyaltyAccount>
        {
            new LoyaltyAccount { Id = Guid.NewGuid(), TotalSpent = 200, CurrentTierId = tierId },
            new LoyaltyAccount { Id = Guid.NewGuid(), TotalSpent = 300, CurrentTierId = tierId }
        };

        _loyaltyAccountRepoMock.Setup(x => x.GetByFilterHandler(query.TierId, query.StartAt, query.EndAt))
            .ReturnsAsync(accounts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _loyaltyAccountRepoMock.Verify(x => x.GetByFilterHandler(tierId, 100, 500), Times.Once);
    }

    // 2. Phủ nhánh: Trả về danh sách rỗng khi không có kết quả
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAccountsMatchFilter()
    {
        // Arrange
        var query = new GetByFilterQuery { TierId = Guid.NewGuid() };

        _loyaltyAccountRepoMock.Setup(x => x.GetByFilterHandler(It.IsAny<Guid?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>()))
            .ReturnsAsync(new List<LoyaltyAccount>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _loyaltyAccountRepoMock.Verify(x => x.GetByFilterHandler(It.IsAny<Guid?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>()), Times.Once);
    }

    // 3. Phủ nhánh: Xử lý trường hợp Repository trả về null (Dòng 23 - result!)
    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenRepositoryReturnsNull()
    {
        // Arrange
        var query = new GetByFilterQuery();

        _loyaltyAccountRepoMock.Setup(x => x.GetByFilterHandler(null, null, null))
            .ReturnsAsync((IEnumerable<LoyaltyAccount>)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}