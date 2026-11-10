using FluentAssertions;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Commands.AutoFillShifts;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using Moq;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class AutoFillShiftsHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock = new();
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock = new();
    private readonly Mock<IGenericRepository<StaffAvailability>> _availabilityRepositoryMock = new();
    private readonly Mock<IIdentityServiceClient> _identityClientMock = new();
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock = new();
    private readonly Mock<IShiftValidationService> _shiftValidationServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoDraftShiftFound()
    {
        var storeId = Guid.NewGuid();
        var command = new AutoFillShiftsCommand
        {
            StoreId = storeId,
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2026, 4, 7)
        };

        _shiftRepositoryMock
            .Setup(x => x.GetShiftsWithAssignmentsAsync(storeId, command.StartDate, command.EndDate, (int)ShiftStatus.Draft, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shift>());

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.FilledCount.Should().Be(0);
        result.TotalSlots.Should().Be(0);
        result.UnfilledShiftIds.Should().BeEmpty();
        _identityClientMock.Verify(x => x.GetAllUsersAsync(It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUnfilled_WhenStoreHasNoStaff()
    {
        var storeId = Guid.NewGuid();
        var shiftArabica = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 3),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            RequiredStaffCount = 2,
            Status = (int)ShiftStatus.Draft,
            ShiftAssignments =
            [
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), IsDeleted = false }
            ]
        };

        var shiftRobusta = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 3),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            RequiredStaffCount = 2,
            Status = (int)ShiftStatus.Draft,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var command = new AutoFillShiftsCommand
        {
            StoreId = storeId,
            StartDate = shiftArabica.ShiftDate,
            EndDate = shiftArabica.ShiftDate
        };

        _shiftRepositoryMock
            .Setup(x => x.GetShiftsWithAssignmentsAsync(storeId, command.StartDate, command.EndDate, (int)ShiftStatus.Draft, It.IsAny<CancellationToken>()))
            .ReturnsAsync([shiftArabica, shiftRobusta]);

        _identityClientMock
            .Setup(x => x.GetAllUsersAsync("staff"))
            .ReturnsAsync(
            [
                new UserInfo { Id = Guid.NewGuid(), HomeStoreId = Guid.NewGuid(), Username = "outside-barista" }
            ]);

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.FilledCount.Should().Be(0);
        result.TotalSlots.Should().Be(3);
        result.UnfilledShiftIds.Should().BeEquivalentTo([shiftArabica.Id, shiftRobusta.Id]);
        _shiftRuleServiceMock.Verify(x => x.GetEffectiveRuleAsync(It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenEveryShiftAlreadyFull()
    {
        var storeId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 5),
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(15, 0),
            RequiredStaffCount = 2,
            Status = (int)ShiftStatus.Draft,
            ShiftAssignments =
            [
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), IsDeleted = false },
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), IsDeleted = false }
            ]
        };

        _shiftRepositoryMock
            .Setup(x => x.GetShiftsWithAssignmentsAsync(storeId, shift.ShiftDate, shift.ShiftDate, (int)ShiftStatus.Draft, It.IsAny<CancellationToken>()))
            .ReturnsAsync([shift]);

        _identityClientMock
            .Setup(x => x.GetAllUsersAsync("staff"))
            .ReturnsAsync(
            [
                new UserInfo { Id = Guid.NewGuid(), HomeStoreId = storeId, Username = "arabica-staff" }
            ]);

        _shiftRuleServiceMock
            .Setup(x => x.GetEffectiveRuleAsync(storeId))
            .ReturnsAsync(new ShiftRule { MinHoursBetweenShifts = 10, MaxHoursPerWeek = 48 });

        _availabilityRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, object>>[]?>(), It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>?>()))
            .ReturnsAsync(new List<StaffAvailability>());

        _shiftRepositoryMock
            .Setup(x => x.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment>());

        var command = new AutoFillShiftsCommand
        {
            StoreId = storeId,
            StartDate = shift.ShiftDate,
            EndDate = shift.ShiftDate
        };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.FilledCount.Should().Be(0);
        result.TotalSlots.Should().Be(0);
        result.UnfilledShiftIds.Should().BeEmpty();
        _assignmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ShiftAssignment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAutoAssignAndTrackUnfilled_WhenCandidatesAreLimited()
    {
        var storeId = Guid.NewGuid();
        var shiftEarly = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 8),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            RequiredStaffCount = 2,
            Status = (int)ShiftStatus.Draft,
            BreakDurationMinutes = 30,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var shiftLate = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 8),
            StartTime = new TimeOnly(14, 30),
            EndTime = new TimeOnly(22, 0),
            RequiredStaffCount = 1,
            Status = (int)ShiftStatus.Draft,
            BreakDurationMinutes = 30,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var staffSuaDa = new UserInfo
        {
            Id = Guid.NewGuid(),
            HomeStoreId = storeId,
            Username = "sua-da-barista",
            CurrentHourlyWage = 22000
        };

        var staffArabica = new UserInfo
        {
            Id = Guid.NewGuid(),
            HomeStoreId = storeId,
            Username = "arabica-barista",
            CurrentHourlyWage = 25000
        };

        var staffRobusta = new UserInfo
        {
            Id = Guid.NewGuid(),
            HomeStoreId = storeId,
            Username = "robusta-barista",
            CurrentHourlyWage = 26000
        };

        var historyShift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = shiftEarly.ShiftDate,
            StartTime = new TimeOnly(0, 0),
            EndTime = new TimeOnly(4, 0),
            BreakDurationMinutes = 0,
            RequiredStaffCount = 1,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var assignmentHistory = new List<ShiftAssignment>
        {
            new() { Id = Guid.NewGuid(), ShiftId = historyShift.Id, UserId = staffSuaDa.Id, Shift = historyShift },
            new() { Id = Guid.NewGuid(), ShiftId = historyShift.Id, UserId = staffArabica.Id, Shift = historyShift },
            new() { Id = Guid.NewGuid(), ShiftId = historyShift.Id, UserId = staffRobusta.Id, Shift = historyShift }
        };

        var addedAssignments = new List<ShiftAssignment>();

        _assignmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShiftAssignment>()))
            .Callback<ShiftAssignment>(a => addedAssignments.Add(a))
            .ReturnsAsync(Guid.NewGuid());

        _shiftRepositoryMock
            .Setup(x => x.GetShiftsWithAssignmentsAsync(storeId, shiftEarly.ShiftDate, shiftEarly.ShiftDate, (int)ShiftStatus.Draft, It.IsAny<CancellationToken>()))
            .ReturnsAsync([shiftLate, shiftEarly]);

        _identityClientMock
            .Setup(x => x.GetAllUsersAsync("staff"))
            .ReturnsAsync([staffSuaDa, staffArabica, staffRobusta]);

        _shiftRuleServiceMock
            .Setup(x => x.GetEffectiveRuleAsync(storeId))
            .ReturnsAsync((ShiftRule?)null);

        _availabilityRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, object>>[]?>(), It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>?>()))
            .ReturnsAsync(new List<StaffAvailability>());

        _shiftRepositoryMock
            .Setup(x => x.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignmentHistory);

        _shiftValidationServiceMock
            .Setup(x => x.ValidateCandidate(It.IsAny<Shift>(), It.IsAny<List<ShiftAssignment>>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns<Shift, List<ShiftAssignment>, int, int>((shift, weekAssignments, _, _) =>
            {
                var userId = weekAssignments.First().UserId;

                if (shift.Id == shiftEarly.Id)
                {
                    if (userId == staffSuaDa.Id)
                    {
                        return new CandidateValidation { IsOverLimit = true, WeeklyHours = 55 };
                    }

                    if (userId == staffArabica.Id)
                    {
                        return new CandidateValidation { WeeklyHours = 8 };
                    }

                    return new CandidateValidation { WeeklyHours = 6 };
                }

                return new CandidateValidation { IsConflict = true, WeeklyHours = 10 };
            });

        var handler = CreateHandler();
        var command = new AutoFillShiftsCommand
        {
            StoreId = storeId,
            StartDate = shiftEarly.ShiftDate,
            EndDate = shiftEarly.ShiftDate
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalSlots.Should().Be(3);
        result.FilledCount.Should().Be(2);
        result.UnfilledShiftIds.Should().ContainSingle().Which.Should().Be(shiftLate.Id);

        addedAssignments.Should().HaveCount(2);
        addedAssignments[0].ShiftId.Should().Be(shiftEarly.Id);
        addedAssignments[0].UserId.Should().Be(staffRobusta.Id);
        addedAssignments[0].Status.Should().Be((int)ShiftAssignmentStatus.Assigned);

        addedAssignments[1].ShiftId.Should().Be(shiftEarly.Id);
        addedAssignments[1].UserId.Should().Be(staffArabica.Id);

        _shiftValidationServiceMock.Verify(
            x => x.ValidateCandidate(It.IsAny<Shift>(), It.IsAny<List<ShiftAssignment>>(), 8, 48),
            Times.AtLeastOnce);
    }

    private AutoFillShiftsHandler CreateHandler()
    {
        return new AutoFillShiftsHandler(
            _shiftRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _availabilityRepositoryMock.Object,
            _identityClientMock.Object,
            _shiftRuleServiceMock.Object,
            _shiftValidationServiceMock.Object);
    }
}
