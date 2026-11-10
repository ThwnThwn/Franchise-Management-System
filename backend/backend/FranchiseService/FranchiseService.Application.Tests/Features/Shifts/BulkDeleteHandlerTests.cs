using FluentAssertions;
using FranchiseService.Application.Features.Shifts.Commands.BulkDelete;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using Moq;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class BulkDeleteHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepoMock = new();
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepoMock = new();

    [Fact]
    public async Task Handle_ShouldThrow_WhenShiftIdsEmpty()
    {
        var handler = new BulkDeleteHandler(_shiftRepoMock.Object, _assignmentRepoMock.Object);
        var command = new BulkDeleteCommand { ShiftIds = new List<Guid>() };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNoDraftShiftFound()
    {
        var command = new BulkDeleteCommand { ShiftIds = [Guid.NewGuid()] };

        _shiftRepoMock
            .Setup(x => x.GetByIdsAsync(command.ShiftIds.ToArray()))
            .ReturnsAsync(
            [
                new Shift
                {
                    Id = command.ShiftIds[0],
                    Status = (int)ShiftStatus.Published,
                    ShiftAssignments = new List<ShiftAssignment>()
                }
            ]);

        var handler = new BulkDeleteHandler(_shiftRepoMock.Object, _assignmentRepoMock.Object);
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldDeleteDraftShiftsAndAssignments()
    {
        var draftShiftId = Guid.NewGuid();
        var publishedShiftId = Guid.NewGuid();

        var command = new BulkDeleteCommand
        {
            ShiftIds = [draftShiftId, publishedShiftId]
        };

        var draftShift = new Shift
        {
            Id = draftShiftId,
            Status = (int)ShiftStatus.Draft,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var publishedShift = new Shift
        {
            Id = publishedShiftId,
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var assignments = new List<ShiftAssignment>
        {
            new() { Id = Guid.NewGuid(), ShiftId = draftShiftId, UserId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), ShiftId = draftShiftId, UserId = Guid.NewGuid() }
        };

        _shiftRepoMock
            .Setup(x => x.GetByIdsAsync(command.ShiftIds.ToArray()))
            .ReturnsAsync([draftShift, publishedShift]);

        _assignmentRepoMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, object>>[]?>(), It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>?>()))
            .ReturnsAsync(assignments);

        _assignmentRepoMock
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _shiftRepoMock
            .Setup(x => x.UpdateAsync(draftShift))
            .ReturnsAsync(1);

        _shiftRepoMock
            .Setup(x => x.DeleteAsync(draftShiftId))
            .Returns(Task.CompletedTask);

        var handler = new BulkDeleteHandler(_shiftRepoMock.Object, _assignmentRepoMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(1);
        draftShift.Status.Should().Be((int)ShiftStatus.Cancelled);

        _assignmentRepoMock.Verify(x => x.DeleteAsync(assignments[0].Id), Times.Once);
        _assignmentRepoMock.Verify(x => x.DeleteAsync(assignments[1].Id), Times.Once);
        _shiftRepoMock.Verify(x => x.UpdateAsync(draftShift), Times.Once);
        _shiftRepoMock.Verify(x => x.DeleteAsync(draftShiftId), Times.Once);
        _shiftRepoMock.Verify(x => x.DeleteAsync(publishedShiftId), Times.Never);
    }
}
