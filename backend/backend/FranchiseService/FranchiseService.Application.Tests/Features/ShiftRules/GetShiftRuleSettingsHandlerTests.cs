using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftRules.Queries.GetShiftRuleSettings;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftRules;

public class GetShiftRuleSettingsHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftRule>> _ruleRepoMock;
    private readonly GetShiftRuleSettingsHandler _handler;

    public GetShiftRuleSettingsHandlerTests()
    {
        _ruleRepoMock = new Mock<IGenericRepository<ShiftRule>>();
        _handler = new GetShiftRuleSettingsHandler(_ruleRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnGlobalAndStoreRules_WhenStoreIdProvided()
    {
        var storeId = Guid.NewGuid();

        var globalRule = new ShiftRule
        {
            Id = Guid.NewGuid(), StoreId = null,
            AllowShuttle = false, AllowShiftSwap = true,
            MinHoursBetweenShifts = 8, MaxHoursPerWeek = 40,
            GracePeriodMinutes = 10, DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var storeRule = new ShiftRule
        {
            Id = Guid.NewGuid(), StoreId = storeId,
            AllowShuttle = true, AllowShiftSwap = true,
            MinHoursBetweenShifts = 10, MaxHoursPerWeek = 48,
            GracePeriodMinutes = 15, DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _ruleRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(new[] { globalRule })
            .ReturnsAsync(new[] { storeRule });

        var query = new GetShiftRuleSettingsQuery { StoreId = storeId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Global.Should().NotBeNull();
        result.Global!.AllowShuttle.Should().BeFalse();
        result.Stores.Should().ContainSingle();
        result.Stores![0].StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNullGlobal_WhenNoGlobalRuleExists()
    {
        _ruleRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftRule>())
            .ReturnsAsync(Enumerable.Empty<ShiftRule>());

        var query = new GetShiftRuleSettingsQuery { StoreId = null };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Global.Should().BeNull();
        result.Stores.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllStoreRules_WhenNoStoreIdFilter()
    {
        var store1 = Guid.NewGuid();
        var store2 = Guid.NewGuid();

        var globalRule = new ShiftRule
        {
            Id = Guid.NewGuid(), StoreId = null,
            MinHoursBetweenShifts = 8, MaxHoursPerWeek = 40,
            GracePeriodMinutes = 10, DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var storeRules = new List<ShiftRule>
        {
            new()
            {
                Id = Guid.NewGuid(), StoreId = store1,
                MinHoursBetweenShifts = 10, MaxHoursPerWeek = 48,
                GracePeriodMinutes = 15, DailyOtThresholdHours = 8,
                WeeklyOtThresholdHours = 40,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(), StoreId = store2,
                MinHoursBetweenShifts = 12, MaxHoursPerWeek = 44,
                GracePeriodMinutes = 10, DailyOtThresholdHours = 8,
                WeeklyOtThresholdHours = 40,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _ruleRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(new[] { globalRule })
            .ReturnsAsync(storeRules);

        var query = new GetShiftRuleSettingsQuery { StoreId = null };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Global.Should().NotBeNull();
        result.Stores.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldMapAllFieldsCorrectly()
    {
        var ruleId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var updatedAt = DateTimeOffset.UtcNow;

        var rule = new ShiftRule
        {
            Id = ruleId, StoreId = storeId,
            AllowShuttle = true, AllowShiftSwap = false,
            MinHoursBetweenShifts = 12, MaxHoursPerWeek = 44,
            GracePeriodMinutes = 20, DailyOtThresholdHours = 10,
            WeeklyOtThresholdHours = 48,
            UpdatedAt = updatedAt
        };

        _ruleRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftRule>())
            .ReturnsAsync(new[] { rule });

        var query = new GetShiftRuleSettingsQuery { StoreId = storeId };
        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Stores![0];
        dto.Id.Should().Be(ruleId);
        dto.StoreId.Should().Be(storeId);
        dto.AllowShuttle.Should().BeTrue();
        dto.AllowShiftSwap.Should().BeFalse();
        dto.MinHoursBetweenShifts.Should().Be(12);
        dto.MaxHoursPerWeek.Should().Be(44);
        dto.GracePeriodMinutes.Should().Be(20);
        dto.DailyOtThresholdHours.Should().Be(10);
        dto.WeeklyOtThresholdHours.Should().Be(48);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}
