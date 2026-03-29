using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.DeleteStaffAvailability;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftAvailabilities;

public class DeleteStaffAvailabilityHandlerTests
{
    private readonly Mock<IGenericRepository<StaffAvailability>> _repoMock;
    private readonly DeleteStaffAvailabilityHandler _handler;

    public DeleteStaffAvailabilityHandlerTests()
    {
        _repoMock = new Mock<IGenericRepository<StaffAvailability>>();
        _handler = new DeleteStaffAvailabilityHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((StaffAvailability?)null);

        var act = () => _handler.Handle(
            new DeleteStaffAvailabilityCommand(id, Guid.NewGuid()), CancellationToken.None);

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
            IsRecurring = true, DayOfWeek = (short)DayOfWeek.Saturday,
            StartTime = new TimeOnly(6, 0), EndTime = new TimeOnly(14, 0),
            IsAvailable = true
        });

        var act = () => _handler.Handle(
            new DeleteStaffAvailabilityCommand(id, otherUserId), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*nguoi khac*");
    }

    [Fact]
    public async Task Handle_ShouldSoftDelete_WhenOwnerDeletes()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new StaffAvailability
        {
            Id = id, UserId = userId,
            IsRecurring = false,
            SpecificDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(22, 0),
            IsAvailable = false,
            Note = "Xin nghi de di thi rang cafe Robusta"
        });

        var result = await _handler.Handle(
            new DeleteStaffAvailabilityCommand(id, userId), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
