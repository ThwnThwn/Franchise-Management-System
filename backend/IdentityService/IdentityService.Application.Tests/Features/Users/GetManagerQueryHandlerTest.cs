using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Queries.GetManager;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class GetManagerQueryHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetManagerQueryHandler _handler;

    public GetManagerQueryHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetManagerQueryHandler(_userRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenManagerNotFound()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var query = new GetManagerQuery(new GetManagerReq { StoreId = storeId });

        // Ép kiểu (User?) để khớp với Nullable Reference Types và tránh Warning
        _userRepoMock.Setup(x => x.GetManagerByStoreIdAsync(storeId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().BeNull();
        _userRepoMock.Verify(x => x.GetManagerByStoreIdAsync(storeId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserRep>(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserRep_WhenManagerExists()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var query = new GetManagerQuery(new GetManagerReq { StoreId = storeId });
        var managerEntity = new User { Id = Guid.NewGuid(), FirstName = "Manager" };
        var userRep = new UserRep { Id = managerEntity.Id, FirstName = "Manager" };

        _userRepoMock.Setup(x => x.GetManagerByStoreIdAsync(storeId))
            .ReturnsAsync(managerEntity);

        _mapperMock.Setup(m => m.Map<UserRep>(managerEntity))
            .Returns(userRep);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(managerEntity.Id);

        _userRepoMock.Verify(x => x.GetManagerByStoreIdAsync(storeId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserRep>(managerEntity), Times.Once);
    }
}