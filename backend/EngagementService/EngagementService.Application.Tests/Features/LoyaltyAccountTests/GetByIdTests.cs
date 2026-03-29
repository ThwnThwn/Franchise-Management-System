using EngagementService.Application.Features.LoyaltyAccount.Queries.GetById;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class GetLoyaltyAccountByIdHandlerTests
{
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyAccountRepoMock;
    private readonly GetByIdHandler _handler;

    public GetLoyaltyAccountByIdHandlerTests()
    {
        _loyaltyAccountRepoMock = new Mock<ILoyaltyAccountRepository>();
        _handler = new GetByIdHandler(_loyaltyAccountRepoMock.Object);
    }

    // 1. Phủ xanh Happy Path: Tìm thấy tài khoản thành công
    [Fact]
    public async Task Handle_ShouldReturnLoyaltyAccount_WhenIdExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var query = new GetByIdQuery(accountId);
        var expectedAccount = new LoyaltyAccount
        {
            Id = accountId,
            AvailablePoints = 100,
            TotalSpent = 1000000
        };

        _loyaltyAccountRepoMock.Setup(x => x.GetByIdAsync(accountId))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(accountId);
        result.AvailablePoints.Should().Be(100);

        _loyaltyAccountRepoMock.Verify(x => x.GetByIdAsync(accountId), Times.Once);
    }

    // 2. Phủ nhánh: Trả về null khi không tìm thấy tài khoản
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var query = new GetByIdQuery(accountId);

        _loyaltyAccountRepoMock.Setup(x => x.GetByIdAsync(accountId))
            .ReturnsAsync((LoyaltyAccount)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _loyaltyAccountRepoMock.Verify(x => x.GetByIdAsync(accountId), Times.Once);
    }
}