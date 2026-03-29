using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Commands.UpdateShiftAssignment;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Application.Signals;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class UpdateShiftAssignmentHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftGenericRepoMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepoMock;
    private readonly Mock<IGenericRepository<ShiftRule>> _ruleRepoMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<IOutboxSignal> _outboxSignalMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly Mock<IShiftValidationService> _validationServiceMock;
    private readonly Mock<IShiftApplicationAutoRejectService> _autoRejectServiceMock;
    private readonly List<OutboxMessage> _outboxMessages;
    private readonly UpdateShiftAssignmentHandler _handler;

    public UpdateShiftAssignmentHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _shiftGenericRepoMock = new Mock<IGenericRepository<Shift>>();
        _assignmentRepoMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _ruleRepoMock = new Mock<IGenericRepository<ShiftRule>>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _outboxSignalMock = new Mock<IOutboxSignal>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _validationServiceMock = new Mock<IShiftValidationService>();
        _autoRejectServiceMock = new Mock<IShiftApplicationAutoRejectService>();
        _outboxMessages = new List<OutboxMessage>();

        _outboxRepositoryMock
            .Setup(r => r.Add(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => _outboxMessages.Add(m));

        _ruleRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftRule, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftRule>());

        _handler = new UpdateShiftAssignmentHandler(
            _shiftRepositoryMock.Object,
            _shiftGenericRepoMock.Object,
            _assignmentRepoMock.Object,
            _ruleRepoMock.Object,
            _outboxRepositoryMock.Object,
            _outboxSignalMock.Object,
            _identityClientMock.Object,
            _validationServiceMock.Object,
            _autoRejectServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftNotFound()
    {
        var command = new UpdateShiftAssignmentCommand
        {
            ShiftId = Guid.NewGuid(),
            UserIds = new List<Guid>()
        };

        _shiftGenericRepoMock
            .Setup(r => r.GetByIdAsync(command.ShiftId))
            .ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.ShiftId}*");
    }

    [Theory]
    [InlineData((int)ShiftStatus.Completed, "*hoan thanh*")]
    [InlineData((int)ShiftStatus.Cancelled, "*da huy*")]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftStatusNotAllowed(int shiftStatus, string expected)
    {
        var shift = CreateShift(requiredStaffCount: 2, status: shiftStatus);
        var command = new UpdateShiftAssignmentCommand { ShiftId = shift.Id, UserIds = new List<Guid>() };

        _shiftGenericRepoMock
            .Setup(r => r.GetByIdAsync(shift.Id))
            .ReturnsAsync(shift);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(expected);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenFinalAssignmentCountExceedsRequired()
    {
        var userArabica = Guid.NewGuid();
        var userRobusta = Guid.NewGuid();
        var userSuaDac = Guid.NewGuid();

        var shift = CreateShift(requiredStaffCount: 2, status: (int)ShiftStatus.Draft);
        var activeAssignments = new List<ShiftAssignment>
        {
            CreateAssignment(shift, userArabica, isDeleted: false),
            CreateAssignment(shift, userRobusta, isDeleted: false)
        };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _shiftRepositoryMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeAssignments);

        var command = new UpdateShiftAssignmentCommand
        {
            ShiftId = shift.Id,
            UserIds = new List<Guid> { userArabica, userRobusta, userSuaDac }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*toi da 2*co 3*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenBlockingWarningsExist()
    {
        var userArabica = Guid.NewGuid();
        var userRobusta = Guid.NewGuid();

        var shift = CreateShift(requiredStaffCount: 2, status: (int)ShiftStatus.Draft);
        var allAssignments = new List<ShiftAssignment>
        {
            CreateAssignment(shift, userArabica, isDeleted: false)
        };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _shiftRepositoryMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allAssignments);

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userRobusta))
            .ReturnsAsync(new UserInfo { Id = userRobusta, CurrentHourlyWage = 43000m, FirstName = "Hao" });

        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, userRobusta, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>
            {
                new() { Code = "OVERLAP", Message = "Trung gio voi ca Arabica", Severity = "BLOCKING" }
            });

        var command = new UpdateShiftAssignmentCommand
        {
            ShiftId = shift.Id,
            UserIds = new List<Guid> { userArabica, userRobusta }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Trung gio voi ca Arabica*");

        _assignmentRepoMock.Verify(r => r.AddAsync(It.IsAny<ShiftAssignment>()), Times.Never);
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnWarnings_WhenWarningExistsAndForceOverrideIsFalse()
    {
        var userArabica = Guid.NewGuid();
        var userRobusta = Guid.NewGuid();

        var shift = CreateShift(requiredStaffCount: 3, status: (int)ShiftStatus.Draft);
        var allAssignments = new List<ShiftAssignment>
        {
            CreateAssignment(shift, userArabica, isDeleted: false)
        };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _shiftRepositoryMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allAssignments);

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userRobusta))
            .ReturnsAsync(new UserInfo { Id = userRobusta, CurrentHourlyWage = 42000m, FirstName = "Minh" });

        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, userRobusta, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>
            {
                new() { Code = "MAX_HOURS_WEEK", Message = "Vuot gio tuan", Severity = "WARNING" }
            });

        var command = new UpdateShiftAssignmentCommand
        {
            ShiftId = shift.Id,
            UserIds = new List<Guid> { userArabica, userRobusta },
            ForceOverride = false
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Code.Should().Be("MAX_HOURS_WEEK");

        _assignmentRepoMock.Verify(r => r.AddAsync(It.IsAny<ShiftAssignment>()), Times.Never);
        _outboxRepositoryMock.Verify(r => r.Add(It.IsAny<OutboxMessage>()), Times.Never);
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRestoreDeletedAssignmentAndPublish_WhenForceOverrideIsTrueAndShiftBecomesFull()
    {
        var managerId = Guid.NewGuid();
        var userArabica = Guid.NewGuid();
        var userRobusta = Guid.NewGuid();

        var shift = CreateShift(requiredStaffCount: 2, status: (int)ShiftStatus.Draft);
        var activeAssignment = CreateAssignment(shift, userArabica, isDeleted: false);
        var deletedAssignment = CreateAssignment(shift, userRobusta, isDeleted: true);

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _shiftRepositoryMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { activeAssignment, deletedAssignment });

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userRobusta))
            .ReturnsAsync(new UserInfo { Id = userRobusta, CurrentHourlyWage = 46000m, FirstName = "Khanh" });

        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, userRobusta, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>
            {
                new() { Code = "REST_TIME", Message = "Nghi chua du", Severity = "WARNING" }
            });

        var command = new UpdateShiftAssignmentCommand
        {
            ShiftId = shift.Id,
            UserIds = new List<Guid> { userArabica, userRobusta },
            ForceOverride = true,
            OverrideReason = "Quat nua buoi, can bo sung ban pha sua dac",
            OverriddenBy = managerId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _shiftRepositoryMock.Verify(r => r.RestoreAssignmentAsync(It.Is<ShiftAssignment>(a =>
            a.UserId == userRobusta &&
            a.IsDeleted == false &&
            a.IsOverridden &&
            a.OverrideReason == command.OverrideReason &&
            a.OverriddenBy == managerId &&
            a.HourlyWage == 46000m), It.IsAny<CancellationToken>()), Times.Once);

        _shiftGenericRepoMock.Verify(r => r.UpdateAsync(It.Is<Shift>(s => s.Id == shift.Id && s.Status == (int)ShiftStatus.Published)), Times.Once);
        _autoRejectServiceMock.Verify(a => a.RejectPendingApplicationsAsync(shift.Id, null, managerId, It.IsAny<CancellationToken>()), Times.Once);
        _outboxMessages.Should().Contain(m => m.Type == "ShiftAssigned");
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRemoveAndAddAssignments_WhenSyncUserListChanges()
    {
        var userArabica = Guid.NewGuid();
        var userRobusta = Guid.NewGuid();
        var userSuaDac = Guid.NewGuid();

        var shift = CreateShift(requiredStaffCount: 3, status: (int)ShiftStatus.Draft);
        var activeArabica = CreateAssignment(shift, userArabica, isDeleted: false);
        var activeRobusta = CreateAssignment(shift, userRobusta, isDeleted: false);

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _shiftRepositoryMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { activeArabica, activeRobusta });

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userSuaDac))
            .ReturnsAsync(new UserInfo { Id = userSuaDac, CurrentHourlyWage = 41000m, FirstName = "An" });

        _validationServiceMock
            .Setup(v => v.ValidateAssignment(shift, userSuaDac, 8, 48, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValidationWarning>());

        var command = new UpdateShiftAssignmentCommand
        {
            ShiftId = shift.Id,
            UserIds = new List<Guid> { userArabica, userSuaDac },
            OverriddenBy = Guid.NewGuid()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _assignmentRepoMock.Verify(r => r.DeleteAsync(activeRobusta.Id), Times.Once);
        _assignmentRepoMock.Verify(r => r.AddAsync(It.Is<ShiftAssignment>(a =>
            a.ShiftId == shift.Id &&
            a.UserId == userSuaDac &&
            a.Status == (int)ShiftAssignmentStatus.Assigned &&
            a.HourlyWage == 41000m)), Times.Once);

        _outboxMessages.Should().Contain(m => m.Type == "ShiftUnassigned");
        _outboxMessages.Should().Contain(m => m.Type == "ShiftAssigned");
        _shiftGenericRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Shift>()), Times.Never);
        _autoRejectServiceMock.Verify(a => a.RejectPendingApplicationsAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxSignalMock.Verify(s => s.Pulse(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenAddedUserDoesNotExistInIdentityService()
    {
        var userArabica = Guid.NewGuid();
        var userUnknown = Guid.NewGuid();

        var shift = CreateShift(requiredStaffCount: 2, status: (int)ShiftStatus.Draft);
        var activeAssignments = new List<ShiftAssignment>
        {
            CreateAssignment(shift, userArabica, isDeleted: false)
        };

        _shiftGenericRepoMock.Setup(r => r.GetByIdAsync(shift.Id)).ReturnsAsync(shift);
        _shiftRepositoryMock
            .Setup(r => r.GetAllAssignmentsIncludingDeletedAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeAssignments);

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userUnknown))
            .ReturnsAsync((UserInfo?)null);

        var command = new UpdateShiftAssignmentCommand
        {
            ShiftId = shift.Id,
            UserIds = new List<Guid> { userArabica, userUnknown }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{userUnknown}*");
    }

    private static Shift CreateShift(int requiredStaffCount, int status)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 9),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 30,
            RequiredStaffCount = requiredStaffCount,
            Status = status,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }

    private static ShiftAssignment CreateAssignment(Shift shift, Guid userId, bool isDeleted)
    {
        return new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userId,
            Status = (int)ShiftAssignmentStatus.Assigned,
            Shift = shift,
            TimeLogs = new List<TimeLog>(),
            IsDeleted = isDeleted,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
