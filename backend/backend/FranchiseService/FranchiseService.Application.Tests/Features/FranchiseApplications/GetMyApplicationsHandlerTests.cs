using FluentAssertions;
using FranchiseService.Application.Features.FranchiseApplications.Queries;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Models;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseApplications;

public class GetMyApplicationsHandlerTests
{
    private readonly Mock<IFranchiseApplicationRepository> _repoMock = new();

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenRepositoryReturnsNull()
    {
        var userId = Guid.NewGuid();

        _repoMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseApplication, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseApplication, bool>>?>()))
            .ReturnsAsync((IEnumerable<FranchiseApplication>?)null!);

        var handler = new GetMyApplicationsHandler(_repoMock.Object);
        var result = await handler.Handle(new GetMyApplicationsQuery { UserId = userId }, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnApplicationsOfManager()
    {
        var managerId = Guid.NewGuid();
        var applications = new List<FranchiseApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ManagerId = managerId,
                StoreName = "Arabica Roastery",
                Contracts = new List<FranchiseContract>()
            }
        };

        _repoMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseApplication, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseApplication, bool>>?>()))
            .ReturnsAsync(applications);

        var handler = new GetMyApplicationsHandler(_repoMock.Object);
        var result = (await handler.Handle(new GetMyApplicationsQuery { UserId = managerId }, CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].StoreName.Should().Be("Arabica Roastery");
    }
}
