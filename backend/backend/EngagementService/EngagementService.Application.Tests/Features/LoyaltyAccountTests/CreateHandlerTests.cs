using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Features.LoyaltyAccount.Commands.Create;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class CreateLoyaltyAccountHandlerTests
{
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyAccountRepoMock;
    private readonly Mock<IMembershipTierRepository> _membershipTierRepoMock;
    private readonly Mock<IMembershipTierCache> _membershipTierCacheMock;
    private readonly Mock<IPointLogRepository> _pointLogRepoMock;
    private readonly Mock<IPointRuleCache> _pointRuleCacheMock;
    private readonly Mock<IPointRuleRepository> _pointRuleRepositoryMock;
    private readonly CreateHandler _handler;

    public CreateLoyaltyAccountHandlerTests()
    {
        _loyaltyAccountRepoMock = new Mock<ILoyaltyAccountRepository>();
        _membershipTierRepoMock = new Mock<IMembershipTierRepository>();
        _membershipTierCacheMock = new Mock<IMembershipTierCache>();
        _pointLogRepoMock = new Mock<IPointLogRepository>();
        _pointRuleCacheMock = new Mock<IPointRuleCache>();
        _pointRuleRepositoryMock = new Mock<IPointRuleRepository>();

        _handler = new CreateHandler(
            _loyaltyAccountRepoMock.Object,
            _membershipTierRepoMock.Object,
            _membershipTierCacheMock.Object,
            _pointLogRepoMock.Object,
            _pointRuleCacheMock.Object,
            _pointRuleRepositoryMock.Object);
    }

    // 1. Phủ nhánh: Tài khoản đã tồn tại (Dòng 33-34)
    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenAccountAlreadyExists()
    {
        var userId = Guid.NewGuid();
        var request = new UpSertReqest { Id = userId };
        _loyaltyAccountRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new LoyaltyAccount());

        var act = () => _handler.Handle(new CreateCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(LoyaltyAccountMsg.AlreadyExists);
    }

    // 2. Phủ nhánh: Không có default tier (Dòng 37-38)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenNoDefaultTier()
    {
        var request = new UpSertReqest { Id = Guid.NewGuid() };
        _loyaltyAccountRepoMock.Setup(x => x.GetByIdAsync(request.Id)).ReturnsAsync((LoyaltyAccount)null!);
        _membershipTierRepoMock.Setup(x => x.GetDefaultTierAsync()).ReturnsAsync((MembershipTier)null!);

        var act = () => _handler.Handle(new CreateCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<DirectoryNotFoundException>().WithMessage(MembershipTierMsg.NotFound);
    }

    // 3. Phủ xanh Happy Path: Có tích điểm và sinh Log (Dòng 40-62)
    [Fact]
    public async Task Handle_ShouldCreateAccountWithPointsAndLog_WhenOrderAmountIsHigh()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpSertReqest { Id = userId, OrderAmount = 100000, OrderId = Guid.NewGuid() };
        var defaultTier = new MembershipTier { Id = Guid.NewGuid(), AmountPerPoint = 1000, MinSpendRequired = 0 };

        _loyaltyAccountRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((LoyaltyAccount)null!);
        _membershipTierRepoMock.Setup(x => x.GetDefaultTierAsync()).ReturnsAsync(defaultTier);

        // Mock cho CalculatePointHelper (Callback MembershipTierCache)
        _membershipTierCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .Returns<Func<Task<List<MembershipTier>>>>(async (func) => await func());
        _membershipTierRepoMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(new List<MembershipTier> { defaultTier });

        // Mock cho HandlePointLog (Callback PointRuleCache) - Dòng 48
        _pointRuleCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>()))
            .Returns<Func<Task<List<PointRule>>>>(async (func) => await func());
        _pointRuleRepositoryMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(new List<PointRule>());

        // Act
        var result = await _handler.Handle(new CreateCommand(request), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _loyaltyAccountRepoMock.Verify(x => x.AddAsync(It.Is<LoyaltyAccount>(a =>
            a.AvailablePoints == 100 &&
            a.PointLogs.Count == 1)), Times.Once);
    }

    // 4. Phủ nhánh: Không tích điểm (Point = 0) (Dòng 45)
    [Fact]
    public async Task Handle_ShouldCreateAccountWithoutLog_WhenPointsIsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpSertReqest { Id = userId, OrderAmount = 500 }; // Thấp hơn AmountPerPoint (1000)
        var defaultTier = new MembershipTier { Id = Guid.NewGuid(), AmountPerPoint = 1000 };

        _loyaltyAccountRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((LoyaltyAccount)null!);
        _membershipTierRepoMock.Setup(x => x.GetDefaultTierAsync()).ReturnsAsync(defaultTier);

        _membershipTierCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .ReturnsAsync(new List<MembershipTier> { defaultTier });

        // Act
        await _handler.Handle(new CreateCommand(request), CancellationToken.None);

        // Assert
        _loyaltyAccountRepoMock.Verify(x => x.AddAsync(It.Is<LoyaltyAccount>(a =>
            a.AvailablePoints == 0 &&
            a.PointLogs.Count == 0)), Times.Once);

        // Đảm bảo không gọi vào PointRuleCache
        _pointRuleCacheMock.Verify(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>()), Times.Never);
    }
}