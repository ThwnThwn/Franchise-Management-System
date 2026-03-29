using FluentAssertions;
using Moq;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.ShiftSwaps.Helpers;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class SwapValidationHelperTests
{
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepoMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepoMock;
    private readonly Mock<IShiftRuleService> _ruleServiceMock;
    private readonly SwapValidationHelper _helper;

    public SwapValidationHelperTests()
    {
        _assignmentRepoMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepoMock = new Mock<IGenericRepository<Shift>>();
        _ruleServiceMock = new Mock<IShiftRuleService>();
        _helper = new SwapValidationHelper(_assignmentRepoMock.Object, _shiftRepoMock.Object, _ruleServiceMock.Object);
    }

    [Fact]
    public async Task ValidatePreconditionsAsync_ShouldThrow_WhenAssignmentNotFound()
    {
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ShiftAssignment?)null);

        var act = () => _helper.ValidatePreconditionsAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong ton tai*");
    }

    [Fact]
    public async Task ValidatePreconditionsAsync_ShouldThrow_WhenUserIsNotOwner()
    {
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        var act = () => _helper.ValidatePreconditionsAsync(assignment.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong duoc phan cong*");
    }

    [Fact]
    public async Task ValidatePreconditionsAsync_ShouldThrow_WhenShiftNotFound()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = userId,
            TimeLogs = new List<TimeLog>()
        };

        _assignmentRepoMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync((Shift?)null);

        var act = () => _helper.ValidatePreconditionsAsync(assignment.Id, userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ca lam viec khong ton tai*");
    }

    [Fact]
    public async Task ValidatePreconditionsAsync_ShouldThrow_WhenShiftNotPublished()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = userId,
            TimeLogs = new List<TimeLog>()
        };
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            Status = (int)ShiftStatus.Draft,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepoMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);

        var act = () => _helper.ValidatePreconditionsAsync(assignment.Id, userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Published*");
    }

    [Fact]
    public async Task ValidatePreconditionsAsync_ShouldThrow_WhenShiftAlreadyStarted()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = userId,
            TimeLogs = new List<TimeLog>()
        };

        var vnNow = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId()));
        var startedAt = vnNow.AddMinutes(-30);
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(startedAt),
            StartTime = TimeOnly.FromDateTime(startedAt),
            EndTime = TimeOnly.FromDateTime(vnNow.AddHours(4)),
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepoMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);

        var act = () => _helper.ValidatePreconditionsAsync(assignment.Id, userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*chua dien ra*");
    }

    [Fact]
    public async Task ValidatePreconditionsAsync_ShouldThrow_WhenRuleDisallowsShiftSwap()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = userId,
            TimeLogs = new List<TimeLog>()
        };
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepoMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _ruleServiceMock.Setup(r => r.GetEffectiveRuleAsync(shift.StoreId)).ReturnsAsync(new ShiftRule { AllowShiftSwap = false });

        var act = () => _helper.ValidatePreconditionsAsync(assignment.Id, userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong cho phep*");
    }

    [Fact]
    public async Task ValidatePreconditionsAsync_ShouldReturnPreCheck_WhenAllConditionsPass()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = userId,
            TimeLogs = new List<TimeLog>()
        };
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };
        var rule = new ShiftRule { Id = Guid.NewGuid(), StoreId = shift.StoreId, AllowShiftSwap = true };

        _assignmentRepoMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _ruleServiceMock.Setup(r => r.GetEffectiveRuleAsync(shift.StoreId)).ReturnsAsync(rule);

        var result = await _helper.ValidatePreconditionsAsync(assignment.Id, userId);

        result.Assignment.Should().Be(assignment);
        result.Shift.Should().Be(shift);
        result.Rule.Should().Be(rule);
    }

    [Fact]
    public void RejectSwap_ShouldUpdateSwapStatusAndAuditFields()
    {
        var swap = new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceAssignmentId = Guid.NewGuid(),
            SourceUserId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Status = (int)SwapRequestStatus.PendingTarget,
            Reason = "Doi ca pha Arabica",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        var reviewerId = Guid.NewGuid();
        SwapValidationHelper.RejectSwap(swap, reviewerId, "Ban target tu choi vi ban dot xuat");

        swap.Status.Should().Be((int)SwapRequestStatus.Rejected);
        swap.ReviewedBy.Should().Be(reviewerId);
        swap.RejectReason.Should().Contain("target tu choi");
        swap.ReviewedAt.Should().NotBeNull();
    }
}

