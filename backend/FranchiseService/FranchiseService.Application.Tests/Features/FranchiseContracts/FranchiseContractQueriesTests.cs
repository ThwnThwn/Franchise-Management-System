using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using FranchiseService.Application.Features.FranchiseContracts.Queries;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class FranchiseContractQueriesTests
{
    private readonly Mock<IFranchiseContractRepository> _repoMock = new();

    [Fact]
    public async Task GetAllContractsHandler_ShouldMapPagedResponse()
    {
        var query = new GetAllContractsQuery
        {
            Page = 1,
            PageSize = 2,
            Status = ContractStatus.Active
        };

        var contracts = new List<FranchiseContract>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = "FR-ARABICA-001",
                Status = ContractStatus.Active,
                Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "FR-ROBUSTA-002",
                Status = ContractStatus.Active,
                Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
            }
        };

        _repoMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, query.Status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<FranchiseContract>(contracts, 4, query.Page, query.PageSize));

        var handler = new GetAllContractsHandler(_repoMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
        result.TotalPages.Should().Be(2);
        result.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task GetContractDetailHandler_ShouldReturnContractDetail()
    {
        var contract = new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Code = "FR-SUADAC-003",
            Application = new FranchiseApplication
            {
                Id = Guid.NewGuid(),
                FullName = "Le Sua Dac",
                Contracts = new List<FranchiseContract>()
            }
        };

        _repoMock
            .Setup(x => x.GetWithApplicationAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var handler = new GetContractDetailHandler(_repoMock.Object);
        var result = await handler.Handle(new GetContractDetailQuery { ContractId = contract.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("FR-SUADAC-003");
        result.Application.FullName.Should().Be("Le Sua Dac");
    }
}
