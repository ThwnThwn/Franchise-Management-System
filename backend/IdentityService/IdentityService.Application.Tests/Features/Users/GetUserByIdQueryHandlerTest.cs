using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Dtos.Loyalty.Response;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Features.Users.Queries.GetUserById;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class GetUserByIdQueryHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IEngagementServiceClient> _engagementMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _engagementMock = new Mock<IEngagementServiceClient>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetUserByIdQueryHandler(
            _userRepoMock.Object,
            _mapperMock.Object,
            _engagementMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new GetUserByIdQuery(userId), default))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnUserWithLoyalty_WhenUserAndLoyaltyExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var user = new User
        {
            Id = userId,
            Email = email,
            Role = new Role { Name = "Admin" }
        };

        var loyaltyRes = new LoyaltyRes
        {
            AvailablePoints = 100,
            TotalSpent = 500,
            CurrentTier = new MembershipTier { Name = "Gold" }
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _engagementMock.Setup(x => x.GetLoyaltyByEmail(email)).ReturnsAsync(loyaltyRes);
        _mapperMock.Setup(m => m.Map<RoleUserRep>(It.IsAny<Role>())).Returns(new RoleUserRep());

        // Act
        var result = await _handler.Handle(new GetUserByIdQuery(userId), default);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.LoyaltyTier.Should().NotBeNull();
        result.LoyaltyTier!.AvailablePoints.Should().Be(100);
        _engagementMock.Verify(x => x.GetLoyaltyByEmail(email), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserWithNullRoleAndLoyalty_WhenTheyAreMissing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "empty@example.com";
        var user = new User { Id = userId, Email = email, Role = null };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _engagementMock.Setup(x => x.GetLoyaltyByEmail(email)).ReturnsAsync((LoyaltyRes?)null!);

        // Act
        var result = await _handler.Handle(new GetUserByIdQuery(userId), default);

        // Assert
        result.Should().NotBeNull();
        result!.Role.Should().BeNull();
        result.LoyaltyTier.Should().BeNull();
        _mapperMock.Verify(m => m.Map<RoleUserRep>(It.IsAny<Role>()), Times.Never);
    }
}