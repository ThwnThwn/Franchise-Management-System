using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Shifts.Commands.CreateShift;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Signals;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class CreateShiftHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IShiftRepository> _shiftQueryRepositoryMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepositoryMock;
    private readonly Mock<IOutboxSignal> _outboxSignalMock;
    private readonly List<Shift> _addedShifts;
    private readonly List<OutboxMessage> _outboxMessages;
    private readonly CreateShiftHandler _handler;

    public CreateShiftHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _shiftQueryRepositoryMock = new Mock<IShiftRepository>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _templateRepositoryMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _addedShifts = new List<Shift>();
        _outboxMessages = new List<OutboxMessage>();

        _shiftRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Shift>()))
            .Callback<Shift>(s => _addedShifts.Add(s))
            .ReturnsAsync(Guid.NewGuid());

        _outboxRepositoryMock
            .Setup(r => r.Add(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => _outboxMessages.Add(m));

        _handler = new CreateShiftHandler(
            _shiftRepositoryMock.Object,
            _shiftQueryRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _outboxSignalMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenTemplateNotFound()
    {
        var templateId = Guid.NewGuid();
        var command = new CreateShiftCommand
        {
            StoreId = Guid.NewGuid(),
            ShiftTemplateId = templateId,
            ShiftDate = new DateOnly(2026, 4, 12),
            RequiredStaffCount = 2,
            Note = "Ca Arabica"
        };

        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync((ShiftTemplate?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{templateId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNoTimeAndNoTemplate()
    {
        var command = new CreateShiftCommand
        {
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 12),
            RequiredStaffCount = 2,
            StartTime = null,
            EndTime = null,
            Note = "Ca khong gio"
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftDuplicated()
    {
        var command = new CreateShiftCommand
        {
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 12),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            RequiredStaffCount = 2,
            Note = "Ca sang Robusta"
        };

        _shiftQueryRepositoryMock
            .Setup(r => r.GetShiftsByDateRangeAsync(command.StoreId, command.ShiftDate, command.ShiftDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Shift
                {
                    Id = Guid.NewGuid(),
                    StoreId = command.StoreId,
                    ShiftDate = command.ShiftDate,
                    StartTime = new TimeOnly(6, 0),
                    EndTime = new TimeOnly(14, 0),
                    ShiftAssignments = new List<ShiftAssignment>()
                }
            });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*đã tồn tại*");
    }

    [Fact]
    public async Task Handle_ShouldCreateShiftFromTemplateAndPublishOutbox()
    {
        var templateId = Guid.NewGuid();
        var command = new CreateShiftCommand
        {
            StoreId = Guid.NewGuid(),
            ShiftTemplateId = templateId,
            ShiftDate = new DateOnly(2026, 4, 13),
            StartTime = null,
            EndTime = null,
            RequiredStaffCount = 3,
            Note = "Ca pha Sua dac",
            BreakDurationMinutes = null
        };

        _templateRepositoryMock
            .Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(new ShiftTemplate
            {
                Id = templateId,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(22, 0),
                BreakDurationMinutes = 30,
                Code = "TPL-SUADAC",
                Name = "Template Sua dac"
            });

        _shiftQueryRepositoryMock
            .Setup(r => r.GetShiftsByDateRangeAsync(command.StoreId, command.ShiftDate, command.ShiftDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Shift>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _addedShifts.Should().HaveCount(1);
        _addedShifts[0].StartTime.Should().Be(new TimeOnly(14, 0));
        _addedShifts[0].EndTime.Should().Be(new TimeOnly(22, 0));
        _addedShifts[0].BreakDurationMinutes.Should().Be(30);
        _outboxMessages.Should().HaveCount(1);
        _outboxMessages[0].Type.Should().Be("ShiftCreated");
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Once);
    }
}
