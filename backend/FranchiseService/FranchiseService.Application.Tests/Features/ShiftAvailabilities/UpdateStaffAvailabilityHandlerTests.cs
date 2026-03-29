using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.UpdateStaffAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftAvailabilities;

public class UpdateStaffAvailabilityHandlerTests
{
    private readonly Mock<IGenericRepository<StaffAvailability>> _repoMock;
    private readonly UpdateStaffAvailabilityHandler _handler;

    public UpdateStaffAvailabilityHandlerTests()
    {
        _repoMock = new Mock<IGenericRepository<StaffAvailability>>();
        _handler = new UpdateStaffAvailabilityHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((StaffAvailability?)null);

        var command = new UpdateStaffAvailabilityCommand(id, Guid.NewGuid(), new UpdateStaffAvailabilityRequest());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenDifferentUser()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new StaffAvailability
        {
            Id = id, UserId = ownerId,
            IsRecurring = true, DayOfWeek = (short)DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0)
        });

        var command = new UpdateStaffAvailabilityCommand(id, otherUserId, new UpdateStaffAvailabilityRequest());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*nguoi khac*");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenSpecificDateMovedToPast()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = false,
            SpecificDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0)
        });

        var command = new UpdateStaffAvailabilityCommand(id, userId, new UpdateStaffAvailabilityRequest
        {
            Note = "Sua lai ghi chu pha Robusta"
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*qua khu*");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenDurationLessThan2Hours()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = false, SpecificDate = futureDate,
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0)
        });

        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new UpdateStaffAvailabilityCommand(id, userId,
            new UpdateStaffAvailabilityRequest
            {
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*2 gio*");
    }

    [Fact]
    public async Task Handle_ShouldSwitchToRecurring_AndClearSpecificDate()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        var entity = new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = false, SpecificDate = futureDate,
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            IsAvailable = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new UpdateStaffAvailabilityCommand(id, userId,
            new UpdateStaffAvailabilityRequest
            {
                IsRecurring = true,
                DayOfWeek = DayOfWeek.Thursday,
                Note = "Moi thu 5 ranh pha ca phe sua da"
            });

        await _handler.Handle(command, CancellationToken.None);

        entity.IsRecurring.Should().BeTrue();
        entity.DayOfWeek.Should().Be((short)DayOfWeek.Thursday);
        entity.SpecificDate.Should().BeNull();
        entity.Note.Should().Be("Moi thu 5 ranh pha ca phe sua da");

        _repoMock.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSwitchToSpecific_AndClearDayOfWeek()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));

        var entity = new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = true, DayOfWeek = (short)DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            IsAvailable = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new UpdateStaffAvailabilityCommand(id, userId,
            new UpdateStaffAvailabilityRequest
            {
                IsRecurring = false,
                SpecificDate = futureDate
            });

        await _handler.Handle(command, CancellationToken.None);

        entity.IsRecurring.Should().BeFalse();
        entity.SpecificDate.Should().Be(futureDate);
        entity.DayOfWeek.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldUpdateDayOfWeek_WhenRecurringNotChanged()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var entity = new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = true, DayOfWeek = (short)DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            IsAvailable = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new UpdateStaffAvailabilityCommand(id, userId,
            new UpdateStaffAvailabilityRequest
            {
                DayOfWeek = DayOfWeek.Friday
            });

        await _handler.Handle(command, CancellationToken.None);

        entity.DayOfWeek.Should().Be((short)DayOfWeek.Friday);
        entity.IsRecurring.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenOverlapAfterUpdate()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        var entity = new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = false, SpecificDate = futureDate,
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            IsAvailable = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(new[]
            {
                new StaffAvailability
                {
                    Id = Guid.NewGuid(), UserId = userId,
                    IsRecurring = false, SpecificDate = futureDate,
                    StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0),
                    IsAvailable = false
                }
            });

        var command = new UpdateStaffAvailabilityCommand(id, userId,
            new UpdateStaffAvailabilityRequest
            {
                StartTime = new TimeOnly(12, 0),
                EndTime = new TimeOnly(20, 0)
            });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*trung*");
    }

    [Fact]
    public async Task Handle_ShouldPartialUpdateFields()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var entity = new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = true, DayOfWeek = (short)DayOfWeek.Wednesday,
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            IsAvailable = true, Note = "Ranh ca sang"
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new UpdateStaffAvailabilityCommand(id, userId,
            new UpdateStaffAvailabilityRequest
            {
                IsAvailable = false,
                Note = "Ban ca sang de di mua Arabica Cau Dat"
            });

        await _handler.Handle(command, CancellationToken.None);

        entity.IsAvailable.Should().BeFalse();
        entity.Note.Should().Be("Ban ca sang de di mua Arabica Cau Dat");
        entity.StartTime.Should().Be(new TimeOnly(8, 0));
        entity.EndTime.Should().Be(new TimeOnly(16, 0));
    }
}
