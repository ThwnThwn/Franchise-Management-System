using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetMyAvailability;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftAvailabilities;

public class GetMyAvailabilityHandlerTests
{
    private readonly Mock<IGenericRepository<StaffAvailability>> _repositoryMock;
    private readonly GetMyAvailabilityHandler _handler;

    public GetMyAvailabilityHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<StaffAvailability>>();
        _handler = new GetMyAvailabilityHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFilterAndPaginateCorrectly()
    {
        var userId = Guid.NewGuid();
        var data = new List<StaffAvailability>
        {
            CreateAvailability(userId, true, false, new DateOnly(2026, 4, 10), null, "Ban pha Arabica", DateTimeOffset.UtcNow.AddHours(-1)),
            CreateAvailability(userId, true, false, new DateOnly(2026, 4, 11), null, "Ban pha Robusta", DateTimeOffset.UtcNow.AddHours(-2)),
            CreateAvailability(userId, false, false, new DateOnly(2026, 4, 11), null, "Ranh pha Sua dac", DateTimeOffset.UtcNow.AddHours(-3)),
            CreateAvailability(userId, true, true, null, (short)DayOfWeek.Friday, "Lich lap lai thu Sau", DateTimeOffset.UtcNow.AddHours(-4))
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(data);

        var query = new GetMyAvailabilityQuery
        {
            UserId = userId,
            IsAvailable = true,
            IsRecurring = false,
            DayOfWeek = DayOfWeek.Friday,
            Keyword = "arabica",
            FromDate = new DateOnly(2026, 4, 1),
            ToDate = new DateOnly(2026, 4, 30),
            Page = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldIncludeRecurringAndSpecificDate_WithDateRangeAndPaging()
    {
        var userId = Guid.NewGuid();
        var data = new List<StaffAvailability>
        {
            CreateAvailability(userId, false, true, null, (short)DayOfWeek.Monday, "Ban ca sang lap lai", DateTimeOffset.UtcNow.AddHours(-1)),
            CreateAvailability(userId, false, false, new DateOnly(2026, 4, 15), null, "Ban ca chieu ngay le", DateTimeOffset.UtcNow.AddHours(-2)),
            CreateAvailability(userId, false, false, new DateOnly(2026, 5, 1), null, "Ban ca ngoai range", DateTimeOffset.UtcNow.AddHours(-3))
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(data);

        var query = new GetMyAvailabilityQuery
        {
            UserId = userId,
            IsAvailable = false,
            FromDate = new DateOnly(2026, 4, 1),
            ToDate = new DateOnly(2026, 4, 30),
            Page = 1,
            PageSize = 2
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.IsRecurring);
        result.Items.Should().Contain(x => x.SpecificDate == new DateOnly(2026, 4, 15));
    }

    private static StaffAvailability CreateAvailability(
        Guid userId,
        bool isAvailable,
        bool isRecurring,
        DateOnly? specificDate,
        short? dayOfWeek,
        string note,
        DateTimeOffset createdAt)
    {
        return new StaffAvailability
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsAvailable = isAvailable,
            IsRecurring = isRecurring,
            SpecificDate = specificDate,
            DayOfWeek = dayOfWeek,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            Note = note,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
}
