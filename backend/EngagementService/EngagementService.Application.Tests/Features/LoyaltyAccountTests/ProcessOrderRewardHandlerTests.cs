using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Features.LoyaltyAccount.Commands.ProcessOrderReward;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class ProcessOrderRewardHandlerTests
{
    private readonly Mock<IMembershipTierCache> _tierCacheMock;
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyRepoMock;
    private readonly Mock<IMembershipTierRepository> _tierRepoMock;
    private readonly Mock<IPointLogRepository> _pointLogRepoMock;
    private readonly Mock<IPointRuleCache> _ruleCacheMock;
    private readonly Mock<IPointRuleRepository> _ruleRepoMock;
    private readonly ProcessOrderRewardHandler _handler;

    public ProcessOrderRewardHandlerTests()
    {
        _tierCacheMock = new Mock<IMembershipTierCache>();
        _loyaltyRepoMock = new Mock<ILoyaltyAccountRepository>();
        _tierRepoMock = new Mock<IMembershipTierRepository>();
        _pointLogRepoMock = new Mock<IPointLogRepository>();
        _ruleCacheMock = new Mock<IPointRuleCache>();
        _ruleRepoMock = new Mock<IPointRuleRepository>();

        _handler = new ProcessOrderRewardHandler(
            _tierCacheMock.Object,
            _loyaltyRepoMock.Object,
            _tierRepoMock.Object,
            _pointLogRepoMock.Object,
            _ruleCacheMock.Object,
            _ruleRepoMock.Object);
    }

    // 1. Happy Path: Tính điểm, nâng hạng thành công và sinh Log (Dòng 34-63)
    [Fact]
    public async Task Handle_ShouldUpdateAccount_UpgradeTier_AndAddPointLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentTierId = Guid.NewGuid();
        var newTierId = Guid.NewGuid();
        var request = new UpSertReqest { Id = userId, OrderAmount = 500000, OrderId = Guid.NewGuid() };

        var loyaltyAccount = new LoyaltyAccount
        {
            Id = userId,
            CurrentTierId = currentTierId,
            TotalSpent = 0,
            AvailablePoints = 0
        };

        var tiers = new List<MembershipTier>
        {
            new MembershipTier { Id = currentTierId, MinSpendRequired = 0, AmountPerPoint = 1000 },
            new MembershipTier { Id = newTierId, MinSpendRequired = 400000, AmountPerPoint = 500 } // Đủ điều kiện nâng lên hạng này
        };

        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(loyaltyAccount);

        // Mock Cache thực thi callback để lấy list Tiers
        _tierCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .Returns<Func<Task<List<MembershipTier>>>>(async (func) => await func());
        _tierRepoMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(tiers);

        // Mock GetMembershipTierAsync cho CalculatePointHelper
        _tierCacheMock.Setup(x => x.GetMembershipTierAsync(currentTierId))
            .ReturnsAsync(tiers[0]);

        // Mock PointRuleCache cho HandlePointLog
        _ruleCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>()))
            .Returns<Func<Task<List<PointRule>>>>(async (func) => await func());
        _ruleRepoMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(new List<PointRule>());

        // Act
        var result = await _handler.Handle(new ProcessOrderRewardCommand(request), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        loyaltyAccount.CurrentTierId.Should().Be(newTierId); // Đã nâng hạng
        loyaltyAccount.TotalSpent.Should().Be(500000);
        loyaltyAccount.AvailablePoints.Should().Be(500); // 500000 / 1000 = 500
        loyaltyAccount.PointLogs.Should().HaveCount(1);

        _loyaltyRepoMock.Verify(x => x.UpdateAsync(loyaltyAccount), Times.Once);
    }

    // 2. Phủ nhánh: Không đủ điểm nâng hạng (Dòng 41-44 vế False)
    [Fact]
    public async Task Handle_ShouldKeepCurrentTier_WhenSpendIsNotEnoughForUpgrade()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentTierId = Guid.NewGuid();
        var request = new UpSertReqest { Id = userId, OrderAmount = 10000 };

        var loyaltyAccount = new LoyaltyAccount { Id = userId, CurrentTierId = currentTierId, TotalSpent = 0 };
        var tier = new MembershipTier { Id = currentTierId, MinSpendRequired = 0, AmountPerPoint = 1000 };

        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(loyaltyAccount);
        _tierCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .ReturnsAsync(new List<MembershipTier> { tier });
        _tierCacheMock.Setup(x => x.GetMembershipTierAsync(currentTierId)).ReturnsAsync(tier);
        _ruleCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>()))
            .ReturnsAsync(new List<PointRule>());

        // Act
        await _handler.Handle(new ProcessOrderRewardCommand(request), CancellationToken.None);

        // Assert
        loyaltyAccount.CurrentTierId.Should().Be(currentTierId); // Giữ nguyên hạng
    }

    // 3. Phủ nhánh: Lỗi khi không tìm thấy Tier nào phù hợp (Dòng 79-80)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenCheckTierReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpSertReqest { Id = userId, OrderAmount = 1000 };
        var loyaltyAccount = new LoyaltyAccount { Id = userId, CurrentTierId = Guid.NewGuid() };

        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(loyaltyAccount);

        // Trả về list Tier rỗng
        _tierCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .ReturnsAsync(new List<MembershipTier>());

        // Act
        var act = () => _handler.Handle(new ProcessOrderRewardCommand(request), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }
}