using FluentAssertions;
using IdentityService.Application.Features.Users.Queries.GetByPhone;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class GetUsersByPhoneHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly GetUsersByPhoneHandler _handler;

    public GetUsersByPhoneHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _handler = new GetUsersByPhoneHandler(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenUsersIsNull()
    {
        // Arrange
        var query = new GetUsersByPhoneQuery("0901234567");

        // Sử dụng ép kiểu (List<User>?) để tránh Warning CS8620
        _userRepoMock.Setup(x => x.GetUsersByPhoneAsync(query.PhoneNumber))
            .ReturnsAsync((List<User>?)null!);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, default))
            .Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage("Không tìm thấy người dùng");
    }

    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenUsersIsEmpty()
    {
        // Arrange
        var query = new GetUsersByPhoneQuery("0901234567");
        _userRepoMock.Setup(x => x.GetUsersByPhoneAsync(query.PhoneNumber))
            .ReturnsAsync(new List<User>()); // Nhánh !users.Any()

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, default))
            .Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage("Không tìm thấy người dùng");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserList_WhenPhoneNumberExists()
    {
        // Arrange
        var phone = "0987654321";
        var query = new GetUsersByPhoneQuery(phone);

        // Khai báo rõ ràng kiểu Nullable để khớp với Interface signature
        List<User>? mockUsers = new List<User>
        {
            new User { Phone = phone, FirstName = "Unit", LastName = "Test" }
        };

        _userRepoMock.Setup(x => x.GetUsersByPhoneAsync(phone))
            .ReturnsAsync(mockUsers);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Phone.Should().Be(phone);

        _userRepoMock.Verify(x => x.GetUsersByPhoneAsync(phone), Times.Once);
    }
}