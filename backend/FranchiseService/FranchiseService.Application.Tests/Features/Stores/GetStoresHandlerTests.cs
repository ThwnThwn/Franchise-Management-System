using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using FranchiseService.Application.Features.Stores.Queries.GetStores;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using Moq;

namespace FranchiseService.Application.Tests.Features.Stores;

public class GetStoresHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMapStoreEntitiesToDtos()
    {
        var repoMock = new Mock<IStoreRepository>();

        repoMock
            .Setup(x => x.GetPagedStoresAsync("Arabica", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Store>(
            [
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Arabica Central",
                    Address = "12 Pho Arabica",
                    Phone = "0909111111",
                    ImageUrl = "https://cdn.cafe.vn/store/arabica-central.jpg",
                    Latitude = 10.77m,
                    Longitude = 106.69m,
                    Status = (int)StoreStatus.Open
                },
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Robusta Garden",
                    Address = "99 Hem Robusta",
                    Phone = "0909222222",
                    Status = (int)StoreStatus.Closed
                }
            ],
            2,
            1,
            10));

        var handler = new GetStoresHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetStoresQuery
            {
                Keyword = "Arabica",
                Page = 1,
                PageSize = 10
            },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Arabica Central");
        result.Items[0].IsOpenNow.Should().BeTrue();
        result.Items[1].IsOpenNow.Should().BeFalse();
    }
}
