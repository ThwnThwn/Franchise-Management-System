using FluentAssertions;
using Moq;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.Shifts.Commands.BulkOpenShifts;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class BulkOpenShiftsHandlerTests
{
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly BulkOpenShiftsHandler _handler;

    public BulkOpenShiftsHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _handler = new BulkOpenShiftsHandler(_shiftRepositoryMock.Object, _assignmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldOpenEligibleShifts_AndSkipInvalidOnes()
    {
        var today = GetVietnamToday();
        var storeId = Guid.NewGuid();

        var openableShift = CreateShift(storeId, today.AddDays(1), (int)ShiftStatus.Draft, requiredStaff: 3);
        var publishedShift = CreateShift(storeId, today.AddDays(1), (int)ShiftStatus.Published, requiredStaff: 3);
        var pastShift = CreateShift(storeId, today.AddDays(-1), (int)ShiftStatus.Draft, requiredStaff: 3);
        var fullShift = CreateShift(storeId, today, (int)ShiftStatus.Draft, requiredStaff: 1);
        var missingShiftId = Guid.NewGuid();

        _shiftRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<Shift, bool>>>() ))
            .ReturnsAsync(new[] { openableShift, publishedShift, pastShift, fullShift });

        _assignmentRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = fullShift.Id, UserId = Guid.NewGuid(), TimeLogs = new List<TimeLog>() }
            });

        var command = new BulkOpenShiftsCommand
        {
            StoreId = storeId,
            ShiftIds = new List<Guid>
            {
                openableShift.Id,
                publishedShift.Id,
                pastShift.Id,
                fullShift.Id,
                missingShiftId
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.OpenedCount.Should().Be(1);
        result.SkippedIds.Should().HaveCount(4);
        result.SkippedIds.Should().Contain(publishedShift.Id);
        result.SkippedIds.Should().Contain(pastShift.Id);
        result.SkippedIds.Should().Contain(fullShift.Id);
        result.SkippedIds.Should().Contain(missingShiftId);
        openableShift.Status.Should().Be((int)ShiftStatus.Open);

        _shiftRepositoryMock.Verify(r => r.UpdateAsync(openableShift), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllSkipped_WhenNoShiftBelongsToStore()
    {
        _shiftRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<Shift, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<Shift>());
        _assignmentRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());

        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new BulkOpenShiftsCommand { StoreId = Guid.NewGuid(), ShiftIds = ids };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.OpenedCount.Should().Be(0);
        result.SkippedIds.Should().BeEquivalentTo(ids);
    }

    private static DateOnly GetVietnamToday()
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);
        return DateOnly.FromDateTime(nowVn);
    }

    private static Shift CreateShift(Guid storeId, DateOnly date, int status, int requiredStaff)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = date,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            RequiredStaffCount = requiredStaff,
            BreakDurationMinutes = 30,
            Status = status,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }
}
