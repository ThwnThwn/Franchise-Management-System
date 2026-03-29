using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.Shifts.Commands.UpdateShift;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Signals;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class UpdateShiftHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IShiftRepository> _shiftQueryRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepositoryMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<IOutboxSignal> _outboxSignalMock;
    private readonly List<OutboxMessage> _outboxMessages;
    private readonly UpdateShiftHandler _handler;

    public UpdateShiftHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _shiftQueryRepositoryMock = new Mock<IShiftRepository>();
        _templateRepositoryMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _outboxMessages = new List<OutboxMessage>();

        _outboxRepositoryMock
            .Setup(r => r.Add(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => _outboxMessages.Add(m));

        _handler = new UpdateShiftHandler(
            _shiftRepositoryMock.Object,
            _shiftQueryRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _outboxSignalMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftNotFound()
    {
        var command = new UpdateShiftCommand { Id = Guid.NewGuid() };
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Theory]
    [InlineData((int)ShiftStatus.Completed, "*hoàn thành*")]
    [InlineData((int)ShiftStatus.Cancelled, "*đã hủy*")]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftNotEditable(int status, string message)
    {
        var shift = CreateShift(status);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new UpdateShiftCommand { Id = shift.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(message);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenTemplateChangedButNotFound()
    {
        var shift = CreateShift((int)ShiftStatus.Draft);
        var templateId = Guid.NewGuid();

        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync((ShiftTemplate?)null);

        var act = () => _handler.Handle(new UpdateShiftCommand
        {
            Id = shift.Id,
            ShiftTemplateId = templateId
        }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenTimeChangedAndDuplicateExists()
    {
        var shift = CreateShift((int)ShiftStatus.Draft);

        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _shiftQueryRepositoryMock
            .Setup(r => r.GetShiftsByDateRangeAsync(shift.StoreId, shift.ShiftDate, shift.ShiftDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Shift
                {
                    Id = Guid.NewGuid(),
                    StoreId = shift.StoreId,
                    ShiftDate = shift.ShiftDate,
                    StartTime = new TimeOnly(7, 0),
                    EndTime = new TimeOnly(15, 0),
                    ShiftAssignments = new List<ShiftAssignment>()
                }
            });

        var command = new UpdateShiftCommand
        {
            Id = shift.Id,
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(15, 0)
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*đã tồn tại*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateShiftAndEmitOutbox_WhenDataValid()
    {
        var shift = CreateShift((int)ShiftStatus.Draft);
        var templateId = Guid.NewGuid();

        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(new ShiftTemplate
            {
                Id = templateId,
                Code = "TPL-ROBUSTA",
                Name = "Template Robusta",
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(23, 0),
                BreakDurationMinutes = 45
            });
        _shiftQueryRepositoryMock
            .Setup(r => r.GetShiftsByDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Shift>());

        var command = new UpdateShiftCommand
        {
            Id = shift.Id,
            ShiftTemplateId = templateId,
            RequiredStaffCount = 4,
            Note = "Cap nhat ca toi pha Sua dac"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        shift.StartTime.Should().Be(new TimeOnly(15, 0));
        shift.EndTime.Should().Be(new TimeOnly(23, 0));
        shift.BreakDurationMinutes.Should().Be(45);
        shift.RequiredStaffCount.Should().Be(4);
        shift.Note.Should().Be("Cap nhat ca toi pha Sua dac");

        _shiftRepositoryMock.Verify(r => r.UpdateAsync(shift), Times.Once);
        _outboxMessages.Should().HaveCount(1);
        _outboxMessages[0].Type.Should().Be("ShiftUpdated");
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Once);
    }

    private static Shift CreateShift(int status)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 14),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            RequiredStaffCount = 2,
            BreakDurationMinutes = 30,
            Status = status,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }
}
