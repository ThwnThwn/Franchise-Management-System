using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.Shifts.Commands.PublishShift;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Signals;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class PublishShiftHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<IOutboxSignal> _outboxSignalMock;
    private readonly PublishShiftHandler _handler;

    public PublishShiftHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _handler = new PublishShiftHandler(
            _shiftRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _outboxSignalMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftNotFound()
    {
        var shiftId = Guid.NewGuid();
        _shiftRepositoryMock
            .Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(new PublishShiftCommand(shiftId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{shiftId}*");
    }

    [Theory]
    [InlineData((int)ShiftStatus.Published, "*publish*")]
    [InlineData((int)ShiftStatus.Completed, "*hoàn thành*")]
    [InlineData((int)ShiftStatus.Cancelled, "*đã hủy*")]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftStatusIsNotDraft(int status, string expectedMessage)
    {
        var shift = CreateShift(status, 2);
        _shiftRepositoryMock
            .Setup(r => r.GetByIdAsync(shift.Id))
            .ReturnsAsync(shift);

        var act = () => _handler.Handle(new PublishShiftCommand(shift.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenAssignedStaffIsNotEnough()
    {
        var shift = CreateShift((int)ShiftStatus.Draft, 3);
        _shiftRepositoryMock
            .Setup(r => r.GetByIdAsync(shift.Id))
            .ReturnsAsync(shift);
        _assignmentRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[] { new ShiftAssignment(), new ShiftAssignment() });

        var act = () => _handler.Handle(new PublishShiftCommand(shift.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*can du 3*hien co 2*");
    }

    [Fact]
    public async Task Handle_ShouldPublishShiftAndEmitOutbox_WhenShiftIsValid()
    {
        var outboxes = new List<OutboxMessage>();
        var shift = CreateShift((int)ShiftStatus.Draft, 2);

        _shiftRepositoryMock
            .Setup(r => r.GetByIdAsync(shift.Id))
            .ReturnsAsync(shift);

        _assignmentRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = Guid.NewGuid() },
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = Guid.NewGuid() }
            });

        _outboxRepositoryMock
            .Setup(r => r.Add(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => outboxes.Add(m));

        var result = await _handler.Handle(new PublishShiftCommand(shift.Id), CancellationToken.None);

        result.Should().Be(Unit.Value);
        shift.Status.Should().Be((int)ShiftStatus.Published);
        outboxes.Should().HaveCount(1);
        outboxes[0].Type.Should().Be("ShiftPublished");
        outboxes[0].Content.Should().NotBeNullOrWhiteSpace();

        _shiftRepositoryMock.Verify(r => r.UpdateAsync(shift), Times.Once);
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Once);
    }

    private static Shift CreateShift(int status, int requiredStaffCount)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 7),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            Status = status,
            RequiredStaffCount = requiredStaffCount,
            BreakDurationMinutes = 30,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }
}
