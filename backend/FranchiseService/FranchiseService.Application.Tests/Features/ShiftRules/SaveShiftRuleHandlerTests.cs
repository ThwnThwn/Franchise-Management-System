using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.ShiftRules.Commands.SaveShiftRule;
using FranchiseService.Application.Features.ShiftRules.DTOs;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftRules;

public class SaveShiftRuleHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftRule>> _ruleRepoMock;
    private readonly Mock<IIdentityServiceClient> _identityMock;
    private readonly SaveShiftRuleHandler _handler;

    public SaveShiftRuleHandlerTests()
    {
        _ruleRepoMock = new Mock<IGenericRepository<ShiftRule>>();
        _identityMock = new Mock<IIdentityServiceClient>();
        _handler = new SaveShiftRuleHandler(_ruleRepoMock.Object, _identityMock.Object);
    }

    private static SaveShiftRuleRequest CreateCoffeeShopRequest(Guid? storeId) => new()
    {
        StoreId = storeId,
        AllowShuttle = true,
        AllowShiftSwap = true,
        MinHoursBetweenShifts = 10,
        MaxHoursPerWeek = 48,
        GracePeriodMinutes = 15,
        DailyOtThresholdHours = 8,
        WeeklyOtThresholdHours = 40
    };

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _identityMock.Setup(i => i.GetUserByIdAsync(userId)).ReturnsAsync((UserInfo?)null);

        var command = new SaveShiftRuleCommand(userId, CreateCoffeeShopRequest(null));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*khong ton tai*");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenStoreManager_TriesToEditOtherStoreRule()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();

        _identityMock.Setup(i => i.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo
            {
                Id = userId, Username = "quanly_robusta",
                FirstName = "Hùng", HomeStoreId = storeId
            });

        var request = CreateCoffeeShopRequest(otherStoreId);
        var command = new SaveShiftRuleCommand(userId, request);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Quan ly*");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenNonAdminTriesToEditGlobalRule()
    {
        var userId = Guid.NewGuid();

        _identityMock.Setup(i => i.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo
            {
                Id = userId, Username = "nhanvien_arabica",
                FirstName = "Loan", HomeStoreId = Guid.NewGuid()
            });

        var request = CreateCoffeeShopRequest(null);
        var command = new SaveShiftRuleCommand(userId, request);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Admin*");
    }

    [Fact]
    public async Task Handle_ShouldCreateNewRule_WhenNoExistingRule()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        _identityMock.Setup(i => i.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo
            {
                Id = userId, Username = "quanly_espresso",
                FirstName = "Bảo", HomeStoreId = storeId
            });

        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftRule>());

        var request = CreateCoffeeShopRequest(storeId);
        var command = new SaveShiftRuleCommand(userId, request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _ruleRepoMock.Verify(r => r.AddAsync(It.Is<ShiftRule>(
            rule => rule.StoreId == storeId
                && rule.AllowShuttle == true
                && rule.MaxHoursPerWeek == 48
                && rule.GracePeriodMinutes == 15
        )), Times.Once);
        _ruleRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ShiftRule>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingRule_WhenRuleAlreadyExists()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var existingRuleId = Guid.NewGuid();

        _identityMock.Setup(i => i.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo
            {
                Id = userId, Username = "quanly_latte",
                FirstName = "Mai", HomeStoreId = storeId
            });

        var existingRule = new ShiftRule
        {
            Id = existingRuleId,
            StoreId = storeId,
            AllowShuttle = false,
            AllowShiftSwap = false,
            MinHoursBetweenShifts = 8,
            MaxHoursPerWeek = 40,
            GracePeriodMinutes = 10,
            DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40
        };

        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(new[] { existingRule });

        var request = CreateCoffeeShopRequest(storeId);
        var command = new SaveShiftRuleCommand(userId, request);

        await _handler.Handle(command, CancellationToken.None);

        existingRule.AllowShuttle.Should().BeTrue();
        existingRule.AllowShiftSwap.Should().BeTrue();
        existingRule.MaxHoursPerWeek.Should().Be(48);
        existingRule.GracePeriodMinutes.Should().Be(15);

        _ruleRepoMock.Verify(r => r.UpdateAsync(existingRule), Times.Once);
        _ruleRepoMock.Verify(r => r.AddAsync(It.IsAny<ShiftRule>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAllowAdmin_ToUpdateGlobalRule()
    {
        var userId = Guid.NewGuid();

        _identityMock.Setup(i => i.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo
            {
                Id = userId, Username = "admin_cfms",
                FirstName = "Admin", HomeStoreId = null
            });

        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftRule>());

        var request = CreateCoffeeShopRequest(null);
        var command = new SaveShiftRuleCommand(userId, request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _ruleRepoMock.Verify(r => r.AddAsync(It.Is<ShiftRule>(
            rule => rule.StoreId == null
        )), Times.Once);
    }
}
