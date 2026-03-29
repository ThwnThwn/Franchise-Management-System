using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftSwaps.Commands.ReleaseShift;
using FranchiseService.Application.Features.ShiftSwaps.Helpers;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class ReleaseShiftHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepoMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepoMock;
    private readonly Mock<IGenericRepository<ShiftSwapRequest>> _swapRepoMock;
    private readonly Mock<IShiftRuleService> _ruleServiceMock;
    private readonly ReleaseShiftHandler _handler;

    public ReleaseShiftHandlerTests()
    {
        _assignmentRepoMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepoMock = new Mock<IGenericRepository<Shift>>();
        _swapRepoMock = new Mock<IGenericRepository<ShiftSwapRequest>>();
        _ruleServiceMock = new Mock<IShiftRuleService>();

        var helper = new SwapValidationHelper(_assignmentRepoMock.Object, _shiftRepoMock.Object, _ruleServiceMock.Object);
        _handler = new ReleaseShiftHandler(helper, _assignmentRepoMock.Object, _shiftRepoMock.Object, _swapRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenThereIsPendingSwap()
    {
        var (assignment, shift) = SetupCommonPreconditions();
        _swapRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftSwapRequest { Id = Guid.NewGuid(), SourceAssignmentId = assignment.Id, Status = (int)SwapRequestStatus.PendingTarget, Reason = "pending" }
            });

        var act = () => _handler.Handle(new ReleaseShiftCommand { AssignmentId = assignment.Id, UserId = assignment.UserId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Dang co yeu cau doi ca*");
    }

    [Fact]
    public async Task Handle_ShouldOpenShift_WhenAfterReleaseNotEnoughStaff()
    {
        var (assignment, shift) = SetupCommonPreconditions(requiredStaff: 2);

        _swapRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftSwapRequest>());
        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = Guid.NewGuid(), TimeLogs = new List<TimeLog>() }
            });

        var result = await _handler.Handle(new ReleaseShiftCommand { AssignmentId = assignment.Id, UserId = assignment.UserId }, CancellationToken.None);

        result.Should().BeTrue();
        shift.Status.Should().Be((int)ShiftStatus.Open);
        _assignmentRepoMock.Verify(r => r.DeleteAsync(assignment.Id), Times.Once);
        _shiftRepoMock.Verify(r => r.UpdateAsync(shift), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldKeepShiftStatus_WhenStillEnoughStaffAfterRelease()
    {
        var (assignment, shift) = SetupCommonPreconditions(requiredStaff: 1);

        _swapRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftSwapRequest>());
        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = Guid.NewGuid(), TimeLogs = new List<TimeLog>() }
            });

        var result = await _handler.Handle(new ReleaseShiftCommand { AssignmentId = assignment.Id, UserId = assignment.UserId }, CancellationToken.None);

        result.Should().BeTrue();
        _shiftRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Shift>()), Times.Never);
    }

    private (ShiftAssignment Assignment, Shift Shift) SetupCommonPreconditions(int requiredStaff = 2)
    {
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            RequiredStaffCount = requiredStaff,
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepoMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _ruleServiceMock.Setup(r => r.GetEffectiveRuleAsync(shift.StoreId)).ReturnsAsync(new ShiftRule { AllowShiftSwap = true });

        return (assignment, shift);
    }
}
