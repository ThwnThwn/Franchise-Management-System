using IdentityService.Application.Dtos.User.Respone;
using FluentAssertions;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class OrderCountUserResTest
{
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