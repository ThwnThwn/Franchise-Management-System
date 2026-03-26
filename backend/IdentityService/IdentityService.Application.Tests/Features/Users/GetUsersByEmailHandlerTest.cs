using FluentAssertions;
using IdentityService.Application.Features.Users.Queries.GetByEmail;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class GetUsersByEmailHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly GetUsersByEmailHandler _handler;

    public GetUsersByEmailHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _handler = new GetUsersByEmailHandler(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenUsersIsNull()
    {
        // Arrange
        var query = new GetUsersByEmailQuery("notfound@test.com");
        _userRepoMock.Setup(x => x.GetUsersByEmailAsync(query.Email))
            .ReturnsAsync((List<User>?)null!); // Nhánh users == null

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, default))
            .Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage("Không tìm thấy người dùng");
    }

    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenUsersIsEmpty()
    {
        // Arrange
        var query = new GetUsersByEmailQuery("empty@test.com");
        _userRepoMock.Setup(x => x.GetUsersByEmailAsync(query.Email))
            .ReturnsAsync(new List<User>()); // Nhánh !users.Any()

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, default))
            .Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage("Không tìm thấy người dùng");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserList_WhenEmailExists()
    {
        // Arrange
        var email = "exist@test.com";
        var query = new GetUsersByEmailQuery(email);
        var userList = new List<User>
        {
            new User { Email = email, FirstName = "Test" }
        };

        _userRepoMock.Setup(x => x.GetUsersByEmailAsync(email))
            .ReturnsAsync(userList);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Email.Should().Be(email);

        _userRepoMock.Verify(x => x.GetUsersByEmailAsync(email), Times.Once);
    }
}