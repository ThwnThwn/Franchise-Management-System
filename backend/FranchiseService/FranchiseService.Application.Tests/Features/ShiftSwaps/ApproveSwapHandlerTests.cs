using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftSwaps.Commands.ApproveSwap;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class ApproveSwapHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftSwapRequest>> _swapRepoMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepoMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepoMock;
    private readonly Mock<IShiftValidationService> _validationServiceMock;
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock;
    private readonly ApproveSwapHandler _handler;

    public ApproveSwapHandlerTests()
    {
        _swapRepoMock = new Mock<IGenericRepository<ShiftSwapRequest>>();
        _assignmentRepoMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepoMock = new Mock<IGenericRepository<Shift>>();
        _validationServiceMock = new Mock<IShiftValidationService>();
        _shiftRuleServiceMock = new Mock<IShiftRuleService>();

        _handler = new ApproveSwapHandler(
            _swapRepoMock.Object,
            _assignmentRepoMock.Object,
            _shiftRepoMock.Object,
            _validationServiceMock.Object,
            _shiftRuleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSwapNotFound()
    {
        var id = Guid.NewGuid();
        _swapRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ShiftSwapRequest?)null);

        var act = () => _handler.Handle(new ApproveSwapCommand { SwapRequestId = id, ReviewedBy = Guid.NewGuid(), Approve = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong ton tai*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSwapNotPendingManager()
    {
        var swap = CreateSwap((int)SwapRequestStatus.PendingTarget);
        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);

        var act = () => _handler.Handle(new ApproveSwapCommand { SwapRequestId = swap.Id, ReviewedBy = Guid.NewGuid(), Approve = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cho Manager duyet*");
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenManagerRejects()
    {
        var reviewerId = Guid.NewGuid();
        var swap = CreateSwap((int)SwapRequestStatus.PendingManager);
        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);

        var result = await _handler.Handle(new ApproveSwapCommand
        {
            SwapRequestId = swap.Id,
            ReviewedBy = reviewerId,
            Approve = false,
            RejectReason = "Manager tu choi vi thieu nguoi"
        }, CancellationToken.None);

        result.Should().BeFalse();
        swap.Status.Should().Be((int)SwapRequestStatus.Rejected);
        swap.ReviewedBy.Should().Be(reviewerId);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectAndThrow_WhenAssignmentMissing()
    {
        var reviewerId = Guid.NewGuid();
        var swap = CreateSwap((int)SwapRequestStatus.PendingManager);

        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(swap.SourceAssignmentId)).ReturnsAsync((ShiftAssignment?)null);

        var act = () => _handler.Handle(new ApproveSwapCommand { SwapRequestId = swap.Id, ReviewedBy = reviewerId, Approve = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*goc khong con ton tai*");

        swap.Status.Should().Be((int)SwapRequestStatus.Rejected);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectAndThrow_WhenShiftAlreadyStarted()
    {
        var reviewerId = Guid.NewGuid();
        var swap = CreateSwap((int)SwapRequestStatus.PendingManager);
        var assignment = new ShiftAssignment
        {
            Id = swap.SourceAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = swap.SourceUserId,
            TimeLogs = new List<TimeLog>()
        };
        var vnNow = DateTime.UtcNow.AddHours(7);
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(vnNow),
            StartTime = new TimeOnly(vnNow.Hour, vnNow.Minute).AddMinutes(-10),
            EndTime = new TimeOnly(vnNow.Hour, vnNow.Minute).AddHours(4),
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(swap.SourceAssignmentId)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new ApproveSwapCommand { SwapRequestId = swap.Id, ReviewedBy = reviewerId, Approve = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*da bat dau*");

        swap.Status.Should().Be((int)SwapRequestStatus.Rejected);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectAndThrow_WhenBlockingValidationExists()
    {
        var reviewerId = Guid.NewGuid();
        var swap = CreateSwap((int)SwapRequestStatus.PendingManager);
        var assignment = new ShiftAssignment
        {
            Id = swap.SourceAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = swap.SourceUserId,
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

        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(swap.SourceAssignmentId)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _shiftRuleServiceMock.Setup(r => r.GetEffectiveRuleAsync(shift.StoreId)).ReturnsAsync(new ShiftRule { MinHoursBetweenShifts = 8, MaxHoursPerWeek = 48 });
        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, swap.TargetUserId, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>
            {
                new() { Code = "OVERLAP", Severity = "BLOCKING", Message = "Trung gio voi ca Latte" }
            });

        var act = () => _handler.Handle(new ApproveSwapCommand { SwapRequestId = swap.Id, ReviewedBy = reviewerId, Approve = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Trung gio voi ca Latte*");

        swap.Status.Should().Be((int)SwapRequestStatus.Rejected);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldApproveAndSwapAssignmentUser_WhenAllValid()
    {
        var reviewerId = Guid.NewGuid();
        var swap = CreateSwap((int)SwapRequestStatus.PendingManager);
        var assignment = new ShiftAssignment
        {
            Id = swap.SourceAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = swap.SourceUserId,
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

        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(swap.SourceAssignmentId)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _shiftRuleServiceMock.Setup(r => r.GetEffectiveRuleAsync(shift.StoreId)).ReturnsAsync(new ShiftRule { MinHoursBetweenShifts = 8, MaxHoursPerWeek = 48 });
        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, swap.TargetUserId, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>());

        var result = await _handler.Handle(new ApproveSwapCommand
        {
            SwapRequestId = swap.Id,
            ReviewedBy = reviewerId,
            Approve = true
        }, CancellationToken.None);

        result.Should().BeTrue();
        assignment.UserId.Should().Be(swap.TargetUserId);
        swap.Status.Should().Be((int)SwapRequestStatus.Approved);
        swap.ReviewedBy.Should().Be(reviewerId);
        _assignmentRepoMock.Verify(r => r.UpdateAsync(assignment), Times.Once);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    private static ShiftSwapRequest CreateSwap(int status)
    {
        return new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceAssignmentId = Guid.NewGuid(),
            SourceUserId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Status = status,
            Reason = "Doi ca cafe",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(12),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}

