using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Shifts.Commands.BulkCreateFromTemplate;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class BulkCreateFromTemplateHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _genericRepoMock;
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepoMock;
    private readonly Mock<IShiftRepository> _shiftRepoMock;
    private readonly BulkCreateFromTemplateHandler _handler;
    private readonly List<Shift> _addedShifts;

    public BulkCreateFromTemplateHandlerTests()
    {
        _genericRepoMock = new Mock<IGenericRepository<Shift>>();
        _templateRepoMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _shiftRepoMock = new Mock<IShiftRepository>();
        _addedShifts = new List<Shift>();

        _genericRepoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Shift>>()))
            .Callback<IEnumerable<Shift>>(s => _addedShifts.AddRange(s));

        _handler = new BulkCreateFromTemplateHandler(
            _genericRepoMock.Object,
            _templateRepoMock.Object,
            _shiftRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTemplateNotFoundOrInactive()
    {
        var command = BuildValidCommand();

        _templateRepoMock.Setup(r => r.GetByIdAsync(command.TemplateId)).ReturnsAsync((ShiftTemplate?)null);
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _templateRepoMock.Setup(r => r.GetByIdAsync(command.TemplateId)).ReturnsAsync(new ShiftTemplate
        {
            Id = command.TemplateId,
            Code = "TPL-INACTIVE",
            Name = "Template tat",
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            IsActive = false
        });

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInputViolatesDateOrStaffRules()
    {
        var command = BuildValidCommand();
        _templateRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) => CreateTemplate(id));

        var invalidDate = BuildValidCommand();
        invalidDate.StartDate = invalidDate.EndDate.AddDays(1);

        var pastDate = BuildValidCommand();
        pastDate.StartDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        pastDate.EndDate = DateOnly.FromDateTime(DateTime.Today);

        var lowStaff = BuildValidCommand();
        lowStaff.RequiredStaffCount = 0;

        var longRange = BuildValidCommand();
        longRange.EndDate = longRange.StartDate.AddDays(91);

        await FluentActions.Invoking(() => _handler.Handle(invalidDate, CancellationToken.None)).Should().ThrowAsync<ArgumentException>();
        await FluentActions.Invoking(() => _handler.Handle(pastDate, CancellationToken.None)).Should().ThrowAsync<ArgumentException>();
        await FluentActions.Invoking(() => _handler.Handle(lowStaff, CancellationToken.None)).Should().ThrowAsync<ArgumentException>();
        await FluentActions.Invoking(() => _handler.Handle(longRange, CancellationToken.None)).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_ShouldCreateShiftsFromTemplate_WhenValid()
    {
        var command = BuildValidCommand();
        var template = CreateTemplate(command.TemplateId);

        _templateRepoMock.Setup(r => r.GetByIdAsync(command.TemplateId)).ReturnsAsync(template);
        _shiftRepoMock
            .Setup(r => r.GetShiftsByDateRangeAsync(command.StoreId, command.StartDate, command.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Shift
                {
                    Id = Guid.NewGuid(),
                    StoreId = command.StoreId,
                    ShiftDate = command.StartDate,
                    StartTime = template.StartTime,
                    EndTime = template.EndTime,
                    ShiftAssignments = new List<ShiftAssignment>()
                }
            });

        var created = await _handler.Handle(command, CancellationToken.None);

        created.Should().BeGreaterThan(0);
        _addedShifts.Should().HaveCount(created);
        _addedShifts.Should().OnlyContain(s => s.ShiftTemplateId == template.Id && s.StartTime == template.StartTime);
        _genericRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Shift>>()), Times.Once);
    }

    private static ShiftTemplate CreateTemplate(Guid templateId)
    {
        return new ShiftTemplate
        {
            Id = templateId,
            Code = "TPL-CA-TOI",
            Name = "Template ca toi Robusta",
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            BreakDurationMinutes = 30,
            IsActive = true
        };
    }

    private static BulkCreateFromTemplateCommand BuildValidCommand()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        return new BulkCreateFromTemplateCommand
        {
            StoreId = Guid.NewGuid(),
            TemplateId = Guid.NewGuid(),
            StartDate = start,
            EndDate = start.AddDays(5),
            RequiredStaffCount = 2
        };
    }
}

