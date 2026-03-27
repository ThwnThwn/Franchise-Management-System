using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Queries.GetUserByIds;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class GetUserByIdsQueryHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetUserByIdsQueryHandler _handler;

    public GetUserByIdsQueryHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetUserByIdsQueryHandler(_userRepoMock.Object, _mapperMock.Object);
    }

    [Theory]
    [InlineData(null)]
    public async Task Handle_ShouldReturnEmptyList_WhenUserIdsIsNullOrEmpty(List<Guid>? ids)
    {
        // Arrange
        // Lưu ý: IReadOnlyList không thể dùng InlineData trực tiếp cho danh sách, 
        // nên ta test null ở đây và Empty ở test case dưới.
        var query = new GetUserByIdsQuery(ids!);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().BeEmpty();
        _userRepoMock.Verify(x => x.GetByIds(It.IsAny<IReadOnlyList<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserIdsIsEmpty()
    {
        // Arrange
        var query = new GetUserByIdsQuery(new List<Guid>());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedUsers_WhenIdsAreValid()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new GetUserByIdsQuery(ids);

        var users = new List<User?>
        {
            new User { Id = ids[0], Username = "user1" },
            new User { Id = ids[1], Username = "user2" }
        };

        _userRepoMock.Setup(x => x.GetByIds(ids)).ReturnsAsync(users);

        _mapperMock.Setup(m => m.Map<UserRep>(It.IsAny<User>()))
                   .Returns((User src) => new UserRep { Id = src.Id });

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().HaveCount(2);
        _userRepoMock.Verify(x => x.GetByIds(ids), Times.Once);
        _mapperMock.Verify(m => m.Map<UserRep>(It.IsAny<User>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldFilterNullUsers_WhenRepositoryReturnsMixedResults()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new GetUserByIdsQuery(ids);

        // Giả lập repo trả về 1 user và 1 null (do không tìm thấy 1 id)
        var users = new List<User?>
        {
            new User { Id = ids[0] },
            null
        };

        _userRepoMock.Setup(x => x.GetByIds(ids)).ReturnsAsync(users);
        _mapperMock.Setup(m => m.Map<UserRep>(It.IsAny<User>())).Returns(new UserRep());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().HaveCount(1); // Chỉ còn 1 vì đã lọc null
        _mapperMock.Verify(m => m.Map<UserRep>(It.IsAny<User>()), Times.Once);
    }
}