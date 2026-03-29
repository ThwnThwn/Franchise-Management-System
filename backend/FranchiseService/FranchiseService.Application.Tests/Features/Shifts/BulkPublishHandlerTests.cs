using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Shifts.Commands.BulkPublish;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class BulkPublishHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly BulkPublishHandler _handler;

    public BulkPublishHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _handler = new BulkPublishHandler(_shiftRepositoryMock.Object, _assignmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenShiftIdsEmpty()
    {
        var command = new BulkPublishCommand { ShiftIds = new List<Guid>() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*it nhat 1 ca*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenNoDraftOrOpenShift()
    {
        var shifts = new[]
        {
            CreateShift((int)ShiftStatus.Published, 1),
            CreateShift((int)ShiftStatus.Completed, 1)
        };

        _shiftRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(shifts);

        var command = new BulkPublishCommand { ShiftIds = shifts.Select(s => s.Id).ToList() };
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Draft*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenAnyShiftNotEnoughStaff()
    {
        var draft = CreateShift((int)ShiftStatus.Draft, 2);
        var open = CreateShift((int)ShiftStatus.Open, 1);

        _shiftRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(new[] { draft, open });

        _assignmentRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = draft.Id, UserId = Guid.NewGuid() },
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = open.Id, UserId = Guid.NewGuid() }
            });

        var command = new BulkPublishCommand { ShiftIds = new List<Guid> { draft.Id, open.Id } };
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{draft.Id}*");
    }

    [Fact]
    public async Task Handle_ShouldPublishOnlyDraftAndOpenShifts_WhenAllHaveEnoughStaff()
    {
        var draft = CreateShift((int)ShiftStatus.Draft, 1);
        var open = CreateShift((int)ShiftStatus.Open, 1);
        var completed = CreateShift((int)ShiftStatus.Completed, 1);

        _shiftRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(new[] { draft, open, completed });

        _assignmentRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = draft.Id, UserId = Guid.NewGuid() },
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = open.Id, UserId = Guid.NewGuid() }
            });

        var command = new BulkPublishCommand { ShiftIds = new List<Guid> { draft.Id, open.Id, completed.Id } };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(2);
        draft.Status.Should().Be((int)ShiftStatus.Published);
        open.Status.Should().Be((int)ShiftStatus.Published);
        completed.Status.Should().Be((int)ShiftStatus.Completed);

        _shiftRepositoryMock.Verify(r => r.UpdateAsync(draft), Times.Once);
        _shiftRepositoryMock.Verify(r => r.UpdateAsync(open), Times.Once);
        _shiftRepositoryMock.Verify(r => r.UpdateAsync(completed), Times.Never);
    }

    private static Shift CreateShift(int status, int required)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 8),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            BreakDurationMinutes = 30,
            RequiredStaffCount = required,
            Status = status,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }
}
