using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftApplications.Queries.GetApplicationsByShift;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftApplications;

public class GetApplicationsByShiftHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftApplication>> _applicationRepositoryMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly Mock<IShiftRepository> _shiftRepoMock;
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock;
    private readonly GetApplicationsByShiftHandler _handler;

    public GetApplicationsByShiftHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IGenericRepository<ShiftApplication>>();
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _shiftRepoMock = new Mock<IShiftRepository>();
        _shiftRuleServiceMock = new Mock<IShiftRuleService>();

        _handler = new GetApplicationsByShiftHandler(
            _applicationRepositoryMock.Object,
            _shiftRepositoryMock.Object,
            _identityClientMock.Object,
            _shiftRepoMock.Object,
            _shiftRuleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoApplications()
    {
        var shiftId = Guid.NewGuid();
        _applicationRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftApplication>());

        var result = await _handler.Handle(new GetApplicationsByShiftQuery { ShiftId = shiftId }, CancellationToken.None);

        result.Should().BeEmpty();
        _identityClientMock.Verify(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldComputeSmartScoreAndSortDescending()
    {
        var shiftId = Guid.NewGuid();
        var userArabica = Guid.NewGuid();
        var userRobusta = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var applications = new List<ShiftApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ShiftId = shiftId,
                UserId = userArabica,
                Status = (int)ShiftApplicationStatus.Pending,
                Note = "Em xin nhan ca Arabica",
                AppliedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ShiftId = shiftId,
                UserId = userRobusta,
                Status = (int)ShiftApplicationStatus.Pending,
                Note = "Em xin nhan ca Robusta",
                AppliedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            }
        };

        var shift = new Shift
        {
            Id = shiftId,
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 10),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 0,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var weeklyShift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 7),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 0,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var arabicaAssignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = weeklyShift.Id,
            UserId = userArabica,
            Shift = weeklyShift,
            TimeLogs = new List<TimeLog>()
        };

        _applicationRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>() ))
            .ReturnsAsync(applications);

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = userArabica, FirstName = "Bao", LastName = "Nguyen", Email = "bao@coffee.vn", CurrentHourlyWage = 25000m },
                new() { Id = userRobusta, FirstName = "Linh", LastName = "Pham", Email = "linh@coffee.vn", CurrentHourlyWage = 50000m }
            });

        _shiftRepositoryMock
            .Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(shift);

        _shiftRuleServiceMock
            .Setup(s => s.GetEffectiveRuleAsync(storeId))
            .ReturnsAsync(new ShiftRule { MaxHoursPerWeek = 48, DailyOtThresholdHours = 8, WeeklyOtThresholdHours = 40 });

        _shiftRepoMock
            .Setup(r => r.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { arabicaAssignment });

        var result = await _handler.Handle(new GetApplicationsByShiftQuery { ShiftId = shiftId }, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].UserId.Should().Be(userArabica);
        result[0].SmartScore.Should().BeGreaterThan(result[1].SmartScore);
        result[0].WeeklyHoursWorked.Should().Be(8);
        result[1].WeeklyHoursWorked.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldUseFallbackValues_WhenShiftNotFoundAndRuleMissing()
    {
        var shiftId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _applicationRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftApplication
                {
                    Id = Guid.NewGuid(),
                    ShiftId = shiftId,
                    UserId = userId,
                    Status = (int)ShiftApplicationStatus.Pending,
                    AppliedAt = DateTimeOffset.UtcNow,
                    Note = "Em xin nhan ca sua dac"
                }
            });

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = userId, FirstName = "An", LastName = null, CurrentHourlyWage = null }
            });

        _shiftRepositoryMock
            .Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync((Shift?)null);

        _shiftRepoMock
            .Setup(r => r.GetAssignmentsByUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());

        var result = await _handler.Handle(new GetApplicationsByShiftQuery { ShiftId = shiftId }, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].UserName.Should().Be("An");
        result[0].HourlyWage.Should().BeNull();
        result[0].SmartScore.Should().BeGreaterThanOrEqualTo(0);
        _shiftRuleServiceMock.Verify(s => s.GetEffectiveRuleAsync(It.IsAny<Guid?>()), Times.Never);
    }
}
