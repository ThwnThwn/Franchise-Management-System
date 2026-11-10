using FluentAssertions;
using Moq;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.ShiftApplications.Commands.ApplyForShift;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftApplications;

public class ApplyForShiftHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftApplication>> _applicationRepositoryMock;
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock;
    private readonly Mock<IShiftValidationService> _validationServiceMock;
    private readonly List<ShiftApplication> _addedApplications;
    private readonly ApplyForShiftHandler _handler;

    public ApplyForShiftHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _applicationRepositoryMock = new Mock<IGenericRepository<ShiftApplication>>();
        _shiftRuleServiceMock = new Mock<IShiftRuleService>();
        _validationServiceMock = new Mock<IShiftValidationService>();
        _addedApplications = new List<ShiftApplication>();

        _applicationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ShiftApplication>()))
            .Callback<ShiftApplication>(a => _addedApplications.Add(a))
            .ReturnsAsync(Guid.NewGuid());

        _handler = new ApplyForShiftHandler(
            _shiftRepositoryMock.Object,
            _applicationRepositoryMock.Object,
            _shiftRuleServiceMock.Object,
            _validationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftNotFound()
    {
        var command = new ApplyForShiftCommand { ShiftId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(command.ShiftId)).ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftNotOpen()
    {
        var shift = CreateShift((int)ShiftStatus.Draft, GetVietnamToday().AddDays(1));
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new ApplyForShiftCommand { ShiftId = shift.Id, UserId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Open*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftDateInPast()
    {
        var shift = CreateShift((int)ShiftStatus.Open, GetVietnamToday().AddDays(-1));
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new ApplyForShiftCommand { ShiftId = shift.Id, UserId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*qua ngay*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenUserAlreadyApplied()
    {
        var userId = Guid.NewGuid();
        var shift = CreateShift((int)ShiftStatus.Open, GetVietnamToday());
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        _applicationRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftApplication { Id = Guid.NewGuid(), ShiftId = shift.Id, UserId = userId, Status = (int)ShiftApplicationStatus.Pending }
            });

        var act = () => _handler.Handle(new ApplyForShiftCommand { ShiftId = shift.Id, UserId = userId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*dang ky ca nay roi*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenValidationWarningsExist()
    {
        var userId = Guid.NewGuid();
        var shift = CreateShift((int)ShiftStatus.Open, GetVietnamToday());
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        _applicationRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftApplication>());

        _shiftRuleServiceMock
            .Setup(s => s.GetEffectiveRuleAsync(shift.StoreId))
            .ReturnsAsync(new ShiftRule { MinHoursBetweenShifts = 8, MaxHoursPerWeek = 48 });

        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, userId, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>
            {
                new() { Code = "OVERLAP", Message = "Trung ca pha Arabica", Severity = "BLOCKING" }
            });

        var act = () => _handler.Handle(new ApplyForShiftCommand
        {
            ShiftId = shift.Id,
            UserId = userId,
            Note = "Em xin nhan ca"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Trung ca pha Arabica*");
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingApplication_WhenInputValid()
    {
        var userId = Guid.NewGuid();
        var shift = CreateShift((int)ShiftStatus.Open, GetVietnamToday());
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);

        _applicationRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftApplication>());

        _shiftRuleServiceMock
            .Setup(s => s.GetEffectiveRuleAsync(shift.StoreId))
            .ReturnsAsync((ShiftRule?)null);

        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, userId, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>());

        var result = await _handler.Handle(new ApplyForShiftCommand
        {
            ShiftId = shift.Id,
            UserId = userId,
            Note = "Em ranh ca Robusta"
        }, CancellationToken.None);

        result.Should().NotBeEmpty();
        _addedApplications.Should().HaveCount(1);
        _addedApplications[0].Id.Should().Be(result);
        _addedApplications[0].Status.Should().Be((int)ShiftApplicationStatus.Pending);
        _addedApplications[0].Note.Should().Be("Em ranh ca Robusta");
    }

    private static DateOnly GetVietnamToday()
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz));
    }

    private static Shift CreateShift(int status, DateOnly date)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = date,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            Status = status,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }
}
