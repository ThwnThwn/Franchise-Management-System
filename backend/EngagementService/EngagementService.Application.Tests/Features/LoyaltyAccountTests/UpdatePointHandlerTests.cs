using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Features.LoyaltyAccount.Commands.UpdatePoint;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;
using SharedKernel.Events;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class UpdatePointHandlerTests
{
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyRepoMock;
    private readonly Mock<IPointLogRepository> _pointLogRepoMock;
    private readonly Mock<IPointRuleCache> _ruleCacheMock;
    private readonly Mock<IPointRuleRepository> _ruleRepoMock;
    private readonly UpdatePointHandler _handler;

    public UpdatePointHandlerTests()
    {
        _loyaltyRepoMock = new Mock<ILoyaltyAccountRepository>();
        _pointLogRepoMock = new Mock<IPointLogRepository>();
        _ruleCacheMock = new Mock<IPointRuleCache>();
        _ruleRepoMock = new Mock<IPointRuleRepository>();

        _handler = new UpdatePointHandler(
            _loyaltyRepoMock.Object,
            _pointLogRepoMock.Object,
            _ruleCacheMock.Object,
            _ruleRepoMock.Object);
    }

    // 1. Phủ nhánh: Không tìm thấy tài khoản (Dòng 34-35)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenAccountNotFound()
    {
        var request = new UpdatePointRequest { Id = Guid.NewGuid() };
        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(request.Id)).ReturnsAsync((LoyaltyAccount)null!);

        var act = () => _handler.Handle(new UpdatePointCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<DirectoryNotFoundException>().WithMessage(LoyaltyAccountMsg.NotFound);
    }

    // 2. Phủ nhánh: Minus - Thành công (Dòng 48-55)
    [Fact]
    public async Task Handle_ShouldDeductPoints_WhenActionIsMinus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdatePointRequest { Id = userId, Point = 50, ActionType = UpdatePointType.Minus, OrderId = Guid.NewGuid() };
        var account = new LoyaltyAccount { Id = userId, AvailablePoints = 100 };

        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(account);

        // Mock Cache thực thi callback để lấy rules
        _ruleCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>()))
            .Returns<Func<Task<List<PointRule>>>>(async (func) => await func());
        _ruleRepoMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(new List<PointRule>());

        // Act
        var result = await _handler.Handle(new UpdatePointCommand(request), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        account.AvailablePoints.Should().Be(50);
        account.PointLogs.Should().HaveCount(1);
        _loyaltyRepoMock.Verify(x => x.UpdateAsync(account), Times.Once);
    }

    // 3. Phủ nhánh: Minus - Thiếu điểm (Dòng 50-51)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenInsufficientPoints()
    {
        var userId = Guid.NewGuid();
        var request = new UpdatePointRequest { Id = userId, Point = 200, ActionType = UpdatePointType.Minus };
        var account = new LoyaltyAccount { Id = userId, AvailablePoints = 100 };

        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(account);
        _ruleCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>())).ReturnsAsync(new List<PointRule>());

        var act = () => _handler.Handle(new UpdatePointCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(LoyaltyAccountMsg.InsufficientPoints);
    }

    // 4. Phủ nhánh: Add - Thành công (Dòng 58-63)
    [Fact]
    public async Task Handle_ShouldAddPoints_WhenActionIsAdd()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdatePointRequest { Id = userId, Point = 50, ActionType = UpdatePointType.Add, OrderId = Guid.NewGuid() };
        var account = new LoyaltyAccount { Id = userId, AvailablePoints = 100 };

        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(account);
        _ruleCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>())).ReturnsAsync(new List<PointRule>());

        // Act
        await _handler.Handle(new UpdatePointCommand(request), CancellationToken.None);

        // Assert
        account.AvailablePoints.Should().Be(150);
        account.PointLogs.Should().HaveCount(1);
    }

    // 5. Phủ nhánh: Default - Action không hợp lệ (Dòng 66-67)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenActionIsInvalid()
    {
        var userId = Guid.NewGuid();
        var request = new UpdatePointRequest { Id = userId, ActionType = (UpdatePointType)999 };
        var account = new LoyaltyAccount { Id = userId };

        _loyaltyRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(account);
        _ruleCacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<PointRule>>>>())).ReturnsAsync(new List<PointRule>());

        var act = () => _handler.Handle(new UpdatePointCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(LoyaltyAccountMsg.InvalidActionType);
    }
}