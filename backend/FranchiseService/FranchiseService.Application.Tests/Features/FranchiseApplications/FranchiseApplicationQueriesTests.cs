using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using FranchiseService.Application.Features.FranchiseApplications.Queries;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseApplications;

public class FranchiseApplicationQueriesTests
{
    private readonly Mock<IFranchiseApplicationRepository> _repoMock = new();

    [Fact]
    public async Task GetAllApplicationsHandler_ShouldMapPagedResult()
    {
        var pageRequest = new GetAllApplicationsQuery
        {
            Page = 2,
            PageSize = 2,
            Status = FranchiseApplicationStatus.Pending,
            Keyword = "Arabica"
        };

        var data = new List<FranchiseApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FullName = "Nguyen Arabica",
                StoreName = "Arabica Prime",
                Status = FranchiseApplicationStatus.Pending,
                Contracts = new List<FranchiseContract>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                FullName = "Tran Robusta",
                StoreName = "Robusta House",
                Status = FranchiseApplicationStatus.Pending,
                Contracts = new List<FranchiseContract>()
            }
        };

        _repoMock
            .Setup(x => x.GetPagedAsync(pageRequest.Page, pageRequest.PageSize, pageRequest.Status, pageRequest.Keyword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<FranchiseApplication>(data, 5, pageRequest.Page, pageRequest.PageSize));

        var handler = new GetAllApplicationsHandler(_repoMock.Object);
        var result = await handler.Handle(pageRequest, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(2);
    }

    [Fact]
    public async Task GetApplicationDetailHandler_ShouldReturnApplicationFromRepository()
    {
        var application = new FranchiseApplication
        {
            Id = Guid.NewGuid(),
            FullName = "Le Sua Dac",
            Email = "suadac@cfms.vn",
            Contracts = new List<FranchiseContract>
            {
                new() { Id = Guid.NewGuid(), ApplicationId = Guid.NewGuid(), Code = "FR-ARABICA-001" }
            }
        };

        _repoMock
            .Setup(x => x.GetWithContractsAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var handler = new GetApplicationDetailHandler(_repoMock.Object);
        var result = await handler.Handle(new GetApplicationDetailQuery { Id = application.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(application.Id);
        result.Contracts.Should().HaveCount(1);
    }
}
