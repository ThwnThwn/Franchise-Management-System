using FluentAssertions;
using FranchiseService.Application.Features.FranchiseContracts.Queries;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Models;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class GetMyContractsHandlerTests
{
    private readonly Mock<IFranchiseContractRepository> _repoMock = new();

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenRepositoryReturnsNull()
    {
        _repoMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseContract, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseContract, bool>>?>()))
            .ReturnsAsync((IEnumerable<FranchiseContract>?)null!);

        var handler = new GetMyContractsHandler(_repoMock.Object);
        var result = await handler.Handle(new GetMyContractsQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnContractsOfManager()
    {
        var managerId = Guid.NewGuid();
        var contracts = new List<FranchiseContract>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = "FR-MY-ARABICA",
                Application = new FranchiseApplication
                {
                    ManagerId = managerId,
                    Contracts = new List<FranchiseContract>()
                }
            }
        };

        _repoMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseContract, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<FranchiseContract, bool>>?>()))
            .ReturnsAsync(contracts);

        var handler = new GetMyContractsHandler(_repoMock.Object);
        var result = (await handler.Handle(new GetMyContractsQuery { UserId = managerId }, CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].Code.Should().Be("FR-MY-ARABICA");
    }
}
