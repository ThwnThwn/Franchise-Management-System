using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Queries.GetAvailableStaffs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class GetAvailableStaffsHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftGenericRepoMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepoMock;
    private readonly Mock<IGenericRepository<ShiftRule>> _ruleRepoMock;
    private readonly Mock<IGenericRepository<StaffAvailability>> _availabilityRepoMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly Mock<IShiftValidationService> _validationServiceMock;
    private readonly GetAvailableStaffsHandler _handler;

    public GetAvailableStaffsHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _shiftGenericRepoMock = new Mock<IGenericRepository<Shift>>();
        _assignmentRepoMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _ruleRepoMock = new Mock<IGenericRepository<ShiftRule>>();
        _availabilityRepoMock = new Mock<IGenericRepository<StaffAvailability>>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _validationServiceMock = new Mock<IShiftValidationService>();

        _handler = new GetAvailableStaffsHandler(
            _shiftRepositoryMock.Object,
            _shiftGenericRepoMock.Object,
            _assignmentRepoMock.Object,
            _ruleRepoMock.Object,
            _availabilityRepoMock.Object,
            _identityClientMock.Object,
            _validationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftNotFound()
    {
        var query = new GetAvailableStaffsQuery(Guid.NewGuid(), Guid.NewGuid());
        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(query.ShiftId)).ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnInternalAndValidExternalCandidates_WithValidationAndShuttleRules()
    {
        var storeId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 22),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var internalUser = new UserInfo { Id = Guid.NewGuid(), HomeStoreId = storeId, FirstName = "Noi bo Arabica", CurrentHourlyWage = 45000m };
        var externalValid = new UserInfo { Id = Guid.NewGuid(), HomeStoreId = Guid.NewGuid(), FirstName = "Ngoai bo Robusta", CurrentHourlyWage = 42000m };
        var externalInvalid = new UserInfo { Id = Guid.NewGuid(), HomeStoreId = Guid.NewGuid(), FirstName = "Ngoai bo Sua dac", CurrentHourlyWage = 40000m };

        var assignmentInternal = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = internalUser.Id, Shift = shift, TimeLogs = new List<TimeLog>() };
        var assignmentExternalValid = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = externalValid.Id, Shift = shift, TimeLogs = new List<TimeLog>() };
        var assignmentExternalInvalid = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = externalInvalid.Id, Shift = shift, TimeLogs = new List<TimeLog>() };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(new List<ShiftRule>
            {
                new() { Id = Guid.NewGuid(), StoreId = null, AllowShuttle = false, MaxHoursPerWeek = 40, MinHoursBetweenShifts = 8 },
                new() { Id = Guid.NewGuid(), StoreId = storeId, AllowShuttle = true, MaxHoursPerWeek = 48, MinHoursBetweenShifts = 10 },
                new() { Id = Guid.NewGuid(), StoreId = externalValid.HomeStoreId, AllowShuttle = true, MaxHoursPerWeek = 48, MinHoursBetweenShifts = 8 },
                new() { Id = Guid.NewGuid(), StoreId = externalInvalid.HomeStoreId, AllowShuttle = false, MaxHoursPerWeek = 48, MinHoursBetweenShifts = 8 }
            });

        _identityClientMock
            .Setup(c => c.GetAllUsersAsync("staff"))
            .ReturnsAsync(new List<UserInfo> { internalUser, externalValid, externalInvalid });

        _shiftRepositoryMock
            .Setup(r => r.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { assignmentInternal, assignmentExternalValid, assignmentExternalInvalid });

        _availabilityRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(new List<StaffAvailability>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = internalUser.Id,
                    IsAvailable = false,
                    IsRecurring = false,
                    SpecificDate = shift.ShiftDate,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(12, 0),
                    Note = "Ban pha Espresso"
                }
            });

        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = internalUser.Id, TimeLogs = new List<TimeLog>() }
            });

        _validationServiceMock
            .Setup(v => v.ValidateCandidate(shift, It.Is<List<ShiftAssignment>>(l => l.Any(a => a.UserId == internalUser.Id)), 10, 48))
            .Returns(new CandidateValidation { IsConflict = true, IsRestTimeInvalid = true, RestHours = 1, IsOverLimit = true, WeeklyHours = 60 });

        _validationServiceMock
            .Setup(v => v.ValidateCandidate(shift, It.Is<List<ShiftAssignment>>(l => l.Any(a => a.UserId == externalValid.Id)), 10, 48))
            .Returns(new CandidateValidation { IsConflict = false, IsRestTimeInvalid = false, RestHours = 12, IsOverLimit = false, WeeklyHours = 16 });

        _validationServiceMock
            .Setup(v => v.ValidateCandidate(shift, It.Is<List<ShiftAssignment>>(l => l.Any(a => a.UserId == externalInvalid.Id)), 10, 48))
            .Returns(new CandidateValidation { IsConflict = false, IsRestTimeInvalid = false, RestHours = 10, IsOverLimit = false, WeeklyHours = 12 });

        var result = await _handler.Handle(new GetAvailableStaffsQuery(storeId, shift.Id), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Id == internalUser.Id && x.IsAssigned && !x.IsAvailable && x.IsConflict);
        result.Should().Contain(x => x.Id == externalValid.Id && !x.Validation!.IsShuttleInvalid);
        result.Should().NotContain(x => x.Id == externalInvalid.Id);
    }

    [Fact]
    public async Task Handle_ShouldUseGlobalRuleFallback_WhenCurrentStoreRuleMissing()
    {
        var storeId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 22),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var staff = new UserInfo { Id = Guid.NewGuid(), HomeStoreId = storeId, FirstName = "Nhan vien Arabica", CurrentHourlyWage = 39000m };
        var assignment = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = staff.Id, Shift = shift, TimeLogs = new List<TimeLog>() };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(new List<ShiftRule>
            {
                new() { Id = Guid.NewGuid(), StoreId = null, AllowShuttle = false, MaxHoursPerWeek = 40, MinHoursBetweenShifts = 8 }
            });

        _identityClientMock.Setup(c => c.GetAllUsersAsync("staff")).ReturnsAsync(new List<UserInfo> { staff });
        _shiftRepositoryMock
            .Setup(r => r.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { assignment });
        _availabilityRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());
        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());

        _validationServiceMock
            .Setup(v => v.ValidateCandidate(shift, It.IsAny<List<ShiftAssignment>>(), 8, 40))
            .Returns(new CandidateValidation { IsConflict = false, IsRestTimeInvalid = false, IsOverLimit = false, WeeklyHours = 8, RestHours = 12 });

        var result = await _handler.Handle(new GetAvailableStaffsQuery(storeId, shift.Id), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(staff.Id);
        result[0].Validation!.WeeklyHours.Should().Be(8);
    }
    [Fact]
    public async Task Handle_ShouldSetAvailabilityNote_WhenOverlappingAvailabilityIsMarkedAvailable()
    {
        var storeId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 23),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var staff = new UserInfo
        {
            Id = Guid.NewGuid(),
            HomeStoreId = storeId,
            FirstName = "Nhan vien Arabica Cold Brew",
            CurrentHourlyWage = 43000m
        };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(new List<ShiftRule>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StoreId = storeId,
                    AllowShuttle = true,
                    MaxHoursPerWeek = 36,
                    MinHoursBetweenShifts = 9
                }
            });

        _identityClientMock.Setup(c => c.GetAllUsersAsync("staff")).ReturnsAsync(new List<UserInfo> { staff });
        _shiftRepositoryMock
            .Setup(r => r.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());
        _availabilityRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(new List<StaffAvailability>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = staff.Id,
                    IsAvailable = true,
                    IsRecurring = false,
                    SpecificDate = shift.ShiftDate,
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(12, 0),
                    Note = "Co the doi sang quay Cold Brew"
                }
            });
        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());

        _validationServiceMock
            .Setup(v => v.ValidateCandidate(shift, It.Is<List<ShiftAssignment>>(a => a.Count == 0), 9, 36))
            .Returns(new CandidateValidation
            {
                IsConflict = false,
                IsRestTimeInvalid = false,
                RestHours = 14,
                IsOverLimit = false,
                WeeklyHours = 18
            });

        var result = await _handler.Handle(new GetAvailableStaffsQuery(storeId, shift.Id), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(staff.Id);
        result[0].IsAvailable.Should().BeTrue();
        result[0].Note.Should().Be("Co the doi sang quay Cold Brew");
        result[0].Validation!.IsShuttleInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultRuleValues_AndFilterOutExternalWhenShuttleNotAllowed()
    {
        var storeId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 24),
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(15, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var externalStaff = new UserInfo
        {
            Id = Guid.NewGuid(),
            HomeStoreId = Guid.NewGuid(),
            FirstName = "Nhan vien shuttle Robusta",
            CurrentHourlyWage = 41000m
        };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _ruleRepoMock.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(new List<ShiftRule>());
        _identityClientMock.Setup(c => c.GetAllUsersAsync("staff")).ReturnsAsync(new List<UserInfo> { externalStaff });
        _shiftRepositoryMock
            .Setup(r => r.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());
        _availabilityRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());
        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());

        _validationServiceMock
            .Setup(v => v.ValidateCandidate(shift, It.Is<List<ShiftAssignment>>(a => a.Count == 0), 8, 48))
            .Returns(new CandidateValidation
            {
                IsConflict = false,
                IsRestTimeInvalid = false,
                RestHours = 16,
                IsOverLimit = false,
                WeeklyHours = 8
            });

        var result = await _handler.Handle(new GetAvailableStaffsQuery(storeId, shift.Id), CancellationToken.None);

        result.Should().BeEmpty();
        _validationServiceMock.Verify(
            v => v.ValidateCandidate(shift, It.Is<List<ShiftAssignment>>(a => a.Count == 0), 8, 48),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAllowExternalWhenGlobalRuleAllowsShuttle_AndHomeStoreRuleIsMissing()
    {
        var storeId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 25),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var externalStaff = new UserInfo
        {
            Id = Guid.NewGuid(),
            HomeStoreId = Guid.NewGuid(),
            FirstName = "Nhan vien pha sua tuoi",
            LastName = "Robusta",
            CurrentHourlyWage = 40500m
        };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(new List<ShiftRule>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StoreId = null,
                    AllowShuttle = true,
                    MaxHoursPerWeek = 52,
                    MinHoursBetweenShifts = 6
                }
            });

        _identityClientMock.Setup(c => c.GetAllUsersAsync("staff")).ReturnsAsync(new List<UserInfo> { externalStaff });
        _shiftRepositoryMock
            .Setup(r => r.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());
        _availabilityRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());
        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());
        _validationServiceMock
            .Setup(v => v.ValidateCandidate(shift, It.Is<List<ShiftAssignment>>(a => a.Count == 0), 6, 52))
            .Returns(new CandidateValidation
            {
                IsConflict = false,
                IsRestTimeInvalid = false,
                RestHours = 20,
                IsOverLimit = false,
                WeeklyHours = 10
            });

        var result = await _handler.Handle(new GetAvailableStaffsQuery(storeId, shift.Id), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(externalStaff.Id);
        result[0].Validation!.IsShuttleInvalid.Should().BeFalse();
        result[0].CurrentHourlyWage.Should().Be(40500m);
    }
}

