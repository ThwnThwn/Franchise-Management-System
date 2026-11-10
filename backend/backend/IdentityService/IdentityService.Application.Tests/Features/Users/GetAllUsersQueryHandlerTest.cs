using AutoMapper;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using IdentityService.Application.Dtos.Loyalty.Response;
using IdentityService.Application.Dtos.Order.Response;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Features.Users.Queries.GetAllUser;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class GetAllUsersQueryHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IEngagementServiceClient> _engagementMock;
    private readonly Mock<ISaleServiceClient> _saleMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _engagementMock = new Mock<IEngagementServiceClient>();
        _saleMock = new Mock<ISaleServiceClient>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetAllUsersQueryHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _mapperMock.Object,
            _engagementMock.Object,
            _saleMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResults_WhenFilteringByLoyaltyAndOrderCount()
    {
        // Arrange
        var query = new GetAllUsersQuery
        {
            TierId = Guid.NewGuid(),
            OrderCount = 5,
            Page = 1,
            PageSize = 10
        };

        var userId = Guid.NewGuid();
        var loyaltyData = new List<LoyaltyRes>
        {
            new LoyaltyRes
            {
                Id = userId,
                AvailablePoints = 100,
                TotalSpent = 500,
                CurrentTier = new MembershipTier
                {
                    Id = Guid.NewGuid(),
                    Name = "Gold",
                    DiscountRate = 0.1
                }
            }
        };
        IEnumerable<UserCountOrderRes> orderData = new List<UserCountOrderRes>
        {
            new UserCountOrderRes
            {
                UserId = userId,
                OrderCount = 5
            }
        };
        var users = new List<User> { new() { Id = userId, Email = "test@test.com" } };
        var pagedUsers = new PagedResult<User>(users, 1, 1, 10);

        _engagementMock.Setup(x => x.GetByFilter(query.TierId, null, null))
            .ReturnsAsync(loyaltyData);

        _saleMock.Setup(x => x.GetCountOrderByUserIds(It.IsAny<List<Guid>>()))
            .ReturnsAsync(orderData);

        _userRepoMock.Setup(x => x.GetPagedUsersAsync(It.IsAny<string>(), It.IsAny<List<Guid>>(), null, null, null, 1, 10, default))
            .ReturnsAsync(pagedUsers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().OrderCount.Should().Be(5);
        result.Items.First().LoyaltyTier.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldWorkCorrectly_WhenNoFiltersApplied()
    {
        // Arrange
        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var users = new List<User> { new() { Id = userId } };
        var pagedUsers = new PagedResult<User>(users, 1, 1, 10);
        IEnumerable<UserCountOrderRes> orderData = new List<UserCountOrderRes>
        {
            new UserCountOrderRes
            {
                UserId = userId,
                OrderCount = 5
            }
        };
        _saleMock.Setup(x => x.GetCountOrderByUserIds(It.IsAny<List<Guid>>()))
             .ReturnsAsync(orderData);

        _userRepoMock.Setup(x => x.GetPagedUsersAsync("", null, null, null, null, 1, 10, default))
            .ReturnsAsync(pagedUsers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        _engagementMock.Verify(x => x.GetByFilter(It.IsAny<Guid?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHandleMissingDataInDictionaries_WithoutCrashing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };
        var users = new List<User> { new() { Id = userId } };
        var pagedUsers = new PagedResult<User>(users, 1, 1, 10);

        IEnumerable<UserCountOrderRes> emptyOrderData = new List<UserCountOrderRes>();

        _saleMock.Setup(x => x.GetCountOrderByUserIds(It.IsAny<List<Guid>>()))
            .ReturnsAsync(emptyOrderData);

        _userRepoMock.Setup(x => x.GetPagedUsersAsync(It.IsAny<string>(), It.IsAny<List<Guid>>(), null, null, null, 1, 10, default))
            .ReturnsAsync(pagedUsers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var item = result.Items.First();
        item.LoyaltyTier.Should().BeNull(); // Dictionaries TryGetValue fail -> null
        item.OrderCount.Should().Be(0);     // Mặc định int là 0
    }
    [Fact]
    public void OrderCountUserRes_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderCount = 10;

        // Act
        var dto = new OrderCountUserRes
        {
            UserId = userId,
            OrderCount = orderCount
        };

        // Assert
        dto.UserId.Should().Be(userId);
        dto.OrderCount.Should().Be(orderCount);
    }
}