using FluentAssertions;
using Moq;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Services;

public class ShiftRuleServiceTests
{
    private readonly Mock<IGenericRepository<ShiftRule>> _ruleRepoMock;
    private readonly ShiftRuleService _service;

    public ShiftRuleServiceTests()
    {
        _ruleRepoMock = new Mock<IGenericRepository<ShiftRule>>();
        _service = new ShiftRuleService(_ruleRepoMock.Object);
    }

    [Fact]
    public async Task GetEffectiveRuleAsync_ShouldReturnStoreRule_WhenStoreRuleExists()
    {
        var storeId = Guid.NewGuid();
        var storeRule = new ShiftRule
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            AllowShuttle = true,
            AllowShiftSwap = true,
            MinHoursBetweenShifts = 10,
            MaxHoursPerWeek = 48,
            GracePeriodMinutes = 15,
            DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40
        };

        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(new[] { storeRule });

        var result = await _service.GetEffectiveRuleAsync(storeId);

        result.Should().NotBeNull();
        result!.StoreId.Should().Be(storeId);
        result.MaxHoursPerWeek.Should().Be(48);
    }

    [Fact]
    public async Task GetEffectiveRuleAsync_ShouldFallbackToGlobalRule_WhenStoreRuleNotFound()
    {
        var storeId = Guid.NewGuid();
        var globalRule = new ShiftRule
        {
            Id = Guid.NewGuid(),
            StoreId = null,
            AllowShuttle = false,
            AllowShiftSwap = false,
            MinHoursBetweenShifts = 8,
            MaxHoursPerWeek = 40,
            GracePeriodMinutes = 10,
            DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40
        };

        _ruleRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftRule>())
            .ReturnsAsync(new[] { globalRule });

        var result = await _service.GetEffectiveRuleAsync(storeId);

        result.Should().NotBeNull();
        result!.StoreId.Should().BeNull();
    }

    [Fact]
    public async Task GetEffectiveRuleAsync_ShouldReturnGlobalRule_WhenStoreIdIsNull()
    {
        var globalRule = new ShiftRule
        {
            Id = Guid.NewGuid(),
            StoreId = null,
            AllowShuttle = true,
            AllowShiftSwap = true,
            MinHoursBetweenShifts = 8,
            MaxHoursPerWeek = 40,
            GracePeriodMinutes = 5,
            DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40
        };

        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(new[] { globalRule });

        var result = await _service.GetEffectiveRuleAsync(null);

        result.Should().NotBeNull();
        result!.StoreId.Should().BeNull();
    }

    [Fact]
    public async Task GetEffectiveRuleAsync_ShouldReturnNull_WhenNoRuleExists()
    {
        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftRule>());

        var result = await _service.GetEffectiveRuleAsync(null);

        result.Should().BeNull();
    }
}
