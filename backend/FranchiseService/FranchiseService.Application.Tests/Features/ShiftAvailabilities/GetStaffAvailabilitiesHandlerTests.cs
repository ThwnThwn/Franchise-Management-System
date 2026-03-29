using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetStaffAvailabilities;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftAvailabilities;

public class GetStaffAvailabilitiesHandlerTests
{
    private readonly Mock<IGenericRepository<StaffAvailability>> _repositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetStaffAvailabilitiesHandler _handler;

    public GetStaffAvailabilitiesHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<StaffAvailability>>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetStaffAvailabilitiesHandler(_repositoryMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenStoreHasNoStaff()
    {
        var storeId = Guid.NewGuid();
        _identityClientMock
            .Setup(c => c.GetAllUsersAsync("staff"))
            .ReturnsAsync(new List<UserInfo>());

        var query = new GetStaffAvailabilitiesQuery
        {
            StoreId = storeId,
            Page = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenUserNotBelongToStore()
    {
        var storeId = Guid.NewGuid();
        var staffInStore = Guid.NewGuid();
        var anotherStaff = Guid.NewGuid();

        _identityClientMock
            .Setup(c => c.GetAllUsersAsync("staff"))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = staffInStore, HomeStoreId = storeId, FirstName = "Hieu" }
            });

        var query = new GetStaffAvailabilitiesQuery
        {
            StoreId = storeId,
            UserId = anotherStaff,
            Page = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterAndPaginate_WhenStoreAndUserMatched()
    {
        var storeId = Guid.NewGuid();
        var staffArabica = Guid.NewGuid();

        _identityClientMock
            .Setup(c => c.GetAllUsersAsync("staff"))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = staffArabica, HomeStoreId = storeId, FirstName = "Trang" }
            });

        var records = new List<StaffAvailability>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = staffArabica,
                IsAvailable = false,
                IsRecurring = false,
                SpecificDate = new DateOnly(2026, 4, 11),
                DayOfWeek = null,
                StartTime = new TimeOnly(6, 0),
                EndTime = new TimeOnly(10, 0),
                Note = "Ban pha Arabica",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = staffArabica,
                IsAvailable = false,
                IsRecurring = true,
                SpecificDate = null,
                DayOfWeek = (short)DayOfWeek.Friday,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(18, 0),
                Note = "Ban lap lai Robusta",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<StaffAvailability, bool>>>() ))
            .ReturnsAsync(records);

        var query = new GetStaffAvailabilitiesQuery
        {
            StoreId = storeId,
            UserId = staffArabica,
            IsAvailable = false,
            Keyword = "arabica",
            FromDate = new DateOnly(2026, 4, 1),
            ToDate = new DateOnly(2026, 4, 30),
            Page = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Note.Should().Be("Ban pha Arabica");
    }

    [Fact]
    public async Task Handle_ShouldFetchAll_WhenNoStoreAndNoUserFilter()
    {
        var records = new List<StaffAvailability>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                IsAvailable = true,
                IsRecurring = false,
                SpecificDate = new DateOnly(2026, 4, 12),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0),
                Note = "Ranh pha Sua dac",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(records);

        var result = await _handler.Handle(new GetStaffAvailabilitiesQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }
}
