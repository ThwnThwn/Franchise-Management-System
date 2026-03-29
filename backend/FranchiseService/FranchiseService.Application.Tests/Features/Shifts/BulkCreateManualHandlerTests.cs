using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Shifts.Commands.BulkCreateManual;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class BulkCreateManualHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _genericRepoMock;
    private readonly Mock<IShiftRepository> _shiftRepoMock;
    private readonly BulkCreateManualHandler _handler;
    private readonly List<Shift> _addedShifts;

    public BulkCreateManualHandlerTests()
    {
        _genericRepoMock = new Mock<IGenericRepository<Shift>>();
        _shiftRepoMock = new Mock<IShiftRepository>();
        _addedShifts = new List<Shift>();

        _genericRepoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Shift>>()))
            .Callback<IEnumerable<Shift>>(s => _addedShifts.AddRange(s));

        _handler = new BulkCreateManualHandler(_genericRepoMock.Object, _shiftRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDateRangeInvalid()
    {
        var command = BuildValidCommand();
        command.StartDate = command.EndDate.AddDays(1);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*bat dau phai truoc*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDaysOfWeekInvalidOrEmpty()
    {
        var emptyDays = BuildValidCommand();
        emptyDays.DaysOfWeek = new List<int>();

        var invalidDay = BuildValidCommand();
        invalidDay.DaysOfWeek = new List<int> { 7 };

        await FluentActions.Invoking(() => _handler.Handle(emptyDays, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();

        await FluentActions.Invoking(() => _handler.Handle(invalidDay, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRequestViolatesBusinessLimits()
    {
        var negativeBreak = BuildValidCommand();
        negativeBreak.BreakDurationMinutes = -1;

        var lowStaff = BuildValidCommand();
        lowStaff.RequiredStaffCount = 0;

        var tooLongRange = BuildValidCommand();
        tooLongRange.EndDate = tooLongRange.StartDate.AddDays(91);

        await FluentActions.Invoking(() => _handler.Handle(negativeBreak, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();

        await FluentActions.Invoking(() => _handler.Handle(lowStaff, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();

        await FluentActions.Invoking(() => _handler.Handle(tooLongRange, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_ShouldCreateOnlyMissingShifts_WhenInputValid()
    {
        var command = BuildValidCommand();

        _shiftRepoMock
            .Setup(r => r.GetShiftsByDateRangeAsync(command.StoreId, command.StartDate, command.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Shift
                {
                    Id = Guid.NewGuid(),
                    StoreId = command.StoreId,
                    ShiftDate = command.StartDate,
                    StartTime = command.StartTime,
                    EndTime = command.EndTime,
                    ShiftAssignments = new List<ShiftAssignment>()
                }
            });

        var created = await _handler.Handle(command, CancellationToken.None);

        created.Should().BeGreaterThan(0);
        _addedShifts.Should().HaveCount(created);
        _addedShifts.Should().OnlyContain(s => s.Status == 0 && s.RequiredStaffCount == command.RequiredStaffCount);
        _genericRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Shift>>()), Times.Once);
    }

    private static BulkCreateManualCommand BuildValidCommand()
    {
        var start = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        return new BulkCreateManualCommand
        {
            StoreId = Guid.NewGuid(),
            StartDate = start,
            EndDate = start.AddDays(7),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            DaysOfWeek = Enumerable.Range(0, 7).ToList(),
            RequiredStaffCount = 2,
            BreakDurationMinutes = 30,
            Note = "Ca thu cong Arabica"
        };
    }
}
