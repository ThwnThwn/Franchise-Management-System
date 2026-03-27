using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Features.Roles.Queries.GetAllRoles;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace IdentityService.Application.Tests.Features.Roles;

public class GetAllRolesQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllRolesQueryHandler _handler;

    public GetAllRolesQueryHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfRoles_WhenRolesExist()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = "Admin" },
            new Role { Id = Guid.NewGuid(), Name = "User" }
        };
        var roleReps = new List<RoleRep>
        {
            new RoleRep { Name = "Admin" },
            new RoleRep { Name = "User" }
        };

        _roleRepositoryMock.Setup(x => x.GetAllAsync(
            It.IsAny<Expression<Func<Role, object>>[]?>(), // includes
            It.IsAny<Expression<Func<Role, bool>>?>()      // predicate
        ))
        .ReturnsAsync(roles);

        _mapperMock.Setup(m => m.Map<IReadOnlyList<RoleRep>>(roles))
            .Returns(roleReps);

        // Act
        var result = await _handler.Handle(new GetAllRolesQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Admin");
        _roleRepositoryMock.Verify(x => x.GetAllAsync(
            It.IsAny<Expression<Func<Role, object>>[]?>(), // includes
            It.IsAny<Expression<Func<Role, bool>>?>()      // predicate
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRolesFound()
    {
        // Arrange
        var emptyRoles = new List<Role>();
        var emptyReps = new List<RoleRep>();

        _roleRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Role, object>>[]?>(), It.IsAny<Expression<Func<Role, bool>>?>()))
            .ReturnsAsync(emptyRoles);

        _mapperMock.Setup(m => m.Map<IReadOnlyList<RoleRep>>(emptyRoles))
            .Returns(emptyReps);

        // Act
        var result = await _handler.Handle(new GetAllRolesQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}