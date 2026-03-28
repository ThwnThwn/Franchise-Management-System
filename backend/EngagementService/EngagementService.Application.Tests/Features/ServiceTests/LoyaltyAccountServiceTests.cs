using EngagementService.Application.Abstraction;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests;

public class LoyaltyAccountServiceTests
{
    private readonly Mock<ILoyaltyAccountRepository> _repoMock;
    private readonly Mock<IPointRuleCache> _cacheMock;

    public LoyaltyAccountServiceTests()
    {
        _repoMock = new Mock<ILoyaltyAccountRepository>();
        _cacheMock = new Mock<IPointRuleCache>();
    }

    // Phủ xanh 100%: Kiểm tra Constructor và khởi tạo Service
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Act
        var service = new LoyaltyAccountService(_repoMock.Object, _cacheMock.Object);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<ILoyaltyAccountService>();
    }
}