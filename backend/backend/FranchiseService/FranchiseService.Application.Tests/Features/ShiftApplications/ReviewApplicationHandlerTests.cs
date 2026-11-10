using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftApplications.Commands.ReviewApplication;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftApplications;

public class ReviewApplicationHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftApplication>> _applicationRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IShiftRepository> _shiftRepoMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock;
    private readonly Mock<IShiftValidationService> _validationServiceMock;
    private readonly Mock<IShiftApplicationAutoRejectService> _autoRejectServiceMock;
    private readonly ReviewApplicationHandler _handler;

    public ReviewApplicationHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IGenericRepository<ShiftApplication>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _shiftRepoMock = new Mock<IShiftRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _shiftRuleServiceMock = new Mock<IShiftRuleService>();
        _validationServiceMock = new Mock<IShiftValidationService>();
        _autoRejectServiceMock = new Mock<IShiftApplicationAutoRejectService>();

        _handler = new ReviewApplicationHandler(
            _applicationRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _shiftRepositoryMock.Object,
            _shiftRepoMock.Object,
            _identityClientMock.Object,
            _shiftRuleServiceMock.Object,
            _validationServiceMock.Object,
            _autoRejectServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenApplicationMissing()
    {
        var id = Guid.NewGuid();
        _applicationRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ShiftApplication?)null);

        var act = () => _handler.Handle(new ReviewApplicationCommand { ApplicationId = id, IsApproved = true, ReviewedBy = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenApplicationNotPending()
    {
        var app = CreateApplication((int)ShiftApplicationStatus.Rejected);
        _applicationRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

        var act = () => _handler.Handle(new ReviewApplicationCommand { ApplicationId = app.Id, IsApproved = true, ReviewedBy = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*xu ly roi*");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenRejectWithoutReason()
    {
        var app = CreateApplication((int)ShiftApplicationStatus.Pending);
        _applicationRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

        var act = () => _handler.Handle(new ReviewApplicationCommand
        {
            ApplicationId = app.Id,
            IsApproved = false,
            ReviewedBy = Guid.NewGuid(),
            ReviewNote = "   "
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ly do*");
    }

    [Fact]
    public async Task Handle_ShouldRejectApplication_WhenIsApprovedFalse()
    {
        var app = CreateApplication((int)ShiftApplicationStatus.Pending);
        _applicationRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(app.ShiftId)).ReturnsAsync(new Shift { Id = app.ShiftId, StoreId = Guid.NewGuid(), ShiftDate = new DateOnly(2026, 4, 20), StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0), ShiftAssignments = new List<ShiftAssignment>() });

        var result = await _handler.Handle(new ReviewApplicationCommand
        {
            ApplicationId = app.Id,
            IsApproved = false,
            ReviewedBy = Guid.NewGuid(),
            ReviewNote = "Ca da du nguoi pha Arabica"
        }, CancellationToken.None);

        result.Should().BeTrue();
        app.Status.Should().Be((int)ShiftApplicationStatus.Rejected);
        app.ReviewNote.Should().Be("Ca da du nguoi pha Arabica");
        _applicationRepositoryMock.Verify(r => r.UpdateAsync(app), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenBlockingWarningExists()
    {
        var app = CreateApplication((int)ShiftApplicationStatus.Pending);
        var shift = CreateShift(requiredStaff: 3);
        app.ShiftId = shift.Id;

        _applicationRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(app.ShiftId)).ReturnsAsync(shift);
        _shiftRepoMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment>());
        _shiftRuleServiceMock
            .Setup(s => s.GetEffectiveRuleAsync(shift.StoreId))
            .ReturnsAsync(new ShiftRule { MinHoursBetweenShifts = 8, MaxHoursPerWeek = 48 });
        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, app.UserId, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>
            {
                new() { Code = "OVERLAP", Severity = "BLOCKING", Message = "Trung ca Robusta" }
            });

        var act = () => _handler.Handle(new ReviewApplicationCommand
        {
            ApplicationId = app.Id,
            IsApproved = true,
            ReviewedBy = Guid.NewGuid()
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Trung ca Robusta*");
    }

    [Fact]
    public async Task Handle_ShouldCreateNewAssignment_WhenApproveAndNoExistingRecord()
    {
        var app = CreateApplication((int)ShiftApplicationStatus.Pending);
        var shift = CreateShift(requiredStaff: 3);
        app.ShiftId = shift.Id;

        var activeAssignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = Guid.NewGuid(),
            Status = (int)ShiftAssignmentStatus.Assigned,
            IsDeleted = false,
            TimeLogs = new List<TimeLog>()
        };

        _applicationRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(app.ShiftId)).ReturnsAsync(shift);
        _shiftRepoMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { activeAssignment });
        _shiftRuleServiceMock
            .Setup(s => s.GetEffectiveRuleAsync(shift.StoreId))
            .ReturnsAsync(new ShiftRule { MinHoursBetweenShifts = 8, MaxHoursPerWeek = 48 });
        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, app.UserId, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>());
        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(app.UserId))
            .ReturnsAsync(new UserInfo { Id = app.UserId, CurrentHourlyWage = 42000m, FirstName = "My" });

        var result = await _handler.Handle(new ReviewApplicationCommand
        {
            ApplicationId = app.Id,
            IsApproved = true,
            ReviewedBy = Guid.NewGuid(),
            ReviewNote = "Duyet bo sung barista"
        }, CancellationToken.None);

        result.Should().BeTrue();
        app.Status.Should().Be((int)ShiftApplicationStatus.Approved);
        _assignmentRepositoryMock.Verify(r => r.AddAsync(It.Is<ShiftAssignment>(a => a.UserId == app.UserId && a.ShiftId == shift.Id)), Times.Once);
        _autoRejectServiceMock.Verify(a => a.RejectPendingApplicationsAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRestoreDeletedAssignmentAndPublishShift_WhenShiftBecomesFull()
    {
        var managerId = Guid.NewGuid();
        var app = CreateApplication((int)ShiftApplicationStatus.Pending);
        var shift = CreateShift(requiredStaff: 2);
        app.ShiftId = shift.Id;

        var activeAssignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = Guid.NewGuid(),
            Status = (int)ShiftAssignmentStatus.Assigned,
            IsDeleted = false,
            TimeLogs = new List<TimeLog>()
        };

        var deletedForUser = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = app.UserId,
            Status = (int)ShiftAssignmentStatus.Assigned,
            IsDeleted = true,
            HourlyWage = 39000m,
            TimeLogs = new List<TimeLog>()
        };

        _applicationRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(app.ShiftId)).ReturnsAsync(shift);
        _shiftRepoMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { activeAssignment, deletedForUser });
        _shiftRuleServiceMock
            .Setup(s => s.GetEffectiveRuleAsync(shift.StoreId))
            .ReturnsAsync(new ShiftRule { MinHoursBetweenShifts = 8, MaxHoursPerWeek = 48 });
        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, app.UserId, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>());
        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(app.UserId))
            .ReturnsAsync(new UserInfo { Id = app.UserId, CurrentHourlyWage = 45000m, FirstName = "Tuan" });

        var result = await _handler.Handle(new ReviewApplicationCommand
        {
            ApplicationId = app.Id,
            IsApproved = true,
            ReviewedBy = managerId,
            ReviewNote = "Duyet nhan su chinh"
        }, CancellationToken.None);

        result.Should().BeTrue();
        deletedForUser.IsDeleted.Should().BeFalse();
        deletedForUser.HourlyWage.Should().Be(45000m);
        shift.Status.Should().Be((int)ShiftStatus.Published);
        _shiftRepoMock.Verify(r => r.RestoreAssignmentAsync(deletedForUser, It.IsAny<CancellationToken>()), Times.Once);
        _shiftRepositoryMock.Verify(r => r.UpdateAsync(shift), Times.Once);
        _autoRejectServiceMock.Verify(a => a.RejectPendingApplicationsAsync(shift.Id, app.Id, managerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ShiftApplication CreateApplication(int status)
    {
        return new ShiftApplication
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = status,
            AppliedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static Shift CreateShift(int requiredStaff)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 20),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            RequiredStaffCount = requiredStaff,
            Status = (int)ShiftStatus.Open,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }
}
