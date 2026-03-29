using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.CreateStaffAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftAvailabilities;

public class CreateStaffAvailabilityHandlerTests
{
    private readonly Mock<IGenericRepository<StaffAvailability>> _repoMock;
    private readonly CreateStaffAvailabilityHandler _handler;

    public CreateStaffAvailabilityHandlerTests()
    {
        _repoMock = new Mock<IGenericRepository<StaffAvailability>>();
        _handler = new CreateStaffAvailabilityHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenSpecificDateIsInPast()
    {
        var command = new CreateStaffAvailabilityCommand(Guid.NewGuid(), new CreateStaffAvailabilityRequest
        {
            IsAvailable = true,
            IsRecurring = false,
            SpecificDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            Note = "Xin nghi de di thu hoach ca phe Arabica"
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*qua khu*");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenDurationLessThan2Hours()
    {
        var command = new CreateStaffAvailabilityCommand(Guid.NewGuid(), new CreateStaffAvailabilityRequest
        {
            IsAvailable = true,
            IsRecurring = true,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Note = "Ranh 1 tieng de xay Robusta"
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*2 gio*");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenOverlapWithExistingRecurring()
    {
        var userId = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(new[]
            {
                new StaffAvailability
                {
                    Id = Guid.NewGuid(), UserId = userId,
                    IsRecurring = true, DayOfWeek = (short)DayOfWeek.Tuesday,
                    IsAvailable = false,
                    StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0)
                }
            });

        var command = new CreateStaffAvailabilityCommand(userId, new CreateStaffAvailabilityRequest
        {
            IsAvailable = true,
            IsRecurring = true,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(18, 0),
            Note = "Ranh de pha ca phe sua da"
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*trung*");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenOverlapWithExistingSpecificDate()
    {
        var userId = Guid.NewGuid();
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(new[]
            {
                new StaffAvailability
                {
                    Id = Guid.NewGuid(), UserId = userId,
                    IsRecurring = false, SpecificDate = targetDate,
                    IsAvailable = false,
                    StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(22, 0)
                }
            });

        var command = new CreateStaffAvailabilityCommand(userId, new CreateStaffAvailabilityRequest
        {
            IsAvailable = true,
            IsRecurring = false,
            SpecificDate = targetDate,
            StartTime = new TimeOnly(20, 0),
            EndTime = new TimeOnly(23, 0),
            Note = "Ranh buoi toi de pha Mocha"
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*trung*");
    }

    [Fact]
    public async Task Handle_ShouldCreateAvailability_WhenRecurringAndNoOverlap()
    {
        var userId = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new CreateStaffAvailabilityCommand(userId, new CreateStaffAvailabilityRequest
        {
            IsAvailable = true,
            IsRecurring = true,
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            Note = "Ranh thu 4 de pha Espresso sang"
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<StaffAvailability>(
            a => a.UserId == userId
                && a.IsRecurring == true
                && a.DayOfWeek == (short)DayOfWeek.Wednesday
                && a.IsAvailable == true
                && a.Note == "Ranh thu 4 de pha Espresso sang"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateAvailability_WhenSpecificDateAndNoOverlap()
    {
        var userId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));

        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new CreateStaffAvailabilityCommand(userId, new CreateStaffAvailabilityRequest
        {
            IsAvailable = false,
            IsRecurring = false,
            SpecificDate = futureDate,
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            Note = "Xin nghi de di mua hat Robusta"
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<StaffAvailability>(
            a => a.IsRecurring == false
                && a.SpecificDate == futureDate
                && a.IsAvailable == false
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipPastDateCheck_WhenRecurring()
    {
        var userId = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<StaffAvailability>());

        var command = new CreateStaffAvailabilityCommand(userId, new CreateStaffAvailabilityRequest
        {
            IsAvailable = true,
            IsRecurring = true,
            DayOfWeek = DayOfWeek.Friday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            Note = "Moi thu 6 em ranh pha Latte"
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
    }
}
