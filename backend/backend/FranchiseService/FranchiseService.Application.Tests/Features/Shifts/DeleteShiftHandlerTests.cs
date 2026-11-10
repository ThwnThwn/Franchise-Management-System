using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.Shifts.Commands.DeleteShift;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Signals;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class DeleteShiftHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<IOutboxSignal> _outboxSignalMock;
    private readonly List<OutboxMessage> _outboxMessages;
    private readonly DeleteShiftHandler _handler;

    public DeleteShiftHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _outboxMessages = new List<OutboxMessage>();

        _outboxRepositoryMock
            .Setup(r => r.Add(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => _outboxMessages.Add(m));

        _handler = new DeleteShiftHandler(
            _shiftRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _outboxSignalMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftMissing()
    {
        var shiftId = Guid.NewGuid();
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shiftId)).ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(new DeleteShiftCommand(shiftId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Theory]
    [InlineData((int)ShiftStatus.Completed, "*hoàn thành*")]
    [InlineData((int)ShiftStatus.Cancelled, "*hủy trước đó*")]
    public async Task Handle_ShouldThrowInvalidOperation_WhenStatusNotAllowed(int status, string message)
    {
        var shift = CreateShift(status);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new DeleteShiftCommand(shift.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(message);
    }

    [Fact]
    public async Task Handle_ShouldCascadeDeleteAssignmentsAndEmitOutbox()
    {
        var shift = CreateShift((int)ShiftStatus.Draft);
        var assignments = new[]
        {
            new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = Guid.NewGuid(), TimeLogs = new List<TimeLog>() },
            new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = Guid.NewGuid(), TimeLogs = new List<TimeLog>() }
        };

        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _assignmentRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(assignments);

        var result = await _handler.Handle(new DeleteShiftCommand(shift.Id), CancellationToken.None);

        result.Should().Be(Unit.Value);
        shift.Status.Should().Be((int)ShiftStatus.Cancelled);
        _assignmentRepositoryMock.Verify(r => r.DeleteAsync(assignments[0].Id), Times.Once);
        _assignmentRepositoryMock.Verify(r => r.DeleteAsync(assignments[1].Id), Times.Once);
        _shiftRepositoryMock.Verify(r => r.UpdateAsync(shift), Times.Once);
        _shiftRepositoryMock.Verify(r => r.DeleteAsync(shift.Id), Times.Once);
        _outboxMessages.Should().ContainSingle(m => m.Type == "ShiftDeleted");
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Once);
    }

    private static Shift CreateShift(int status)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 15),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            RequiredStaffCount = 2,
            BreakDurationMinutes = 30,
            Status = status,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }
}
