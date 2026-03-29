using FluentAssertions;
using FranchiseService.Application.Features.Stores.Queries.FindNearbyStores;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using Moq;

namespace FranchiseService.Application.Tests.Features.Stores;

public class FindNearbyStoresHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMappedNearbyStores()
    {
        var repoMock = new Mock<IStoreRepository>();

        repoMock
            .Setup(x => x.GetNearbyStoresAsync(10.78, 106.69, 3.5, 2))
            .ReturnsAsync(
            [
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Sua Da Roastery",
                    Address = "1 Duong Sua Da",
                    Phone = "0909333333",
                    ImageUrl = "https://cdn.cafe.vn/store/suada.jpg",
                    Latitude = 10.781m,
                    Longitude = 106.691m,
                    Status = (int)StoreStatus.Open
                },
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Robusta Lab",
                    Address = "2 Pho Robusta",
                    Phone = "0909444444",
                    Status = (int)StoreStatus.Renovating
                }
            ]);

        var handler = new FindNearbyStoresHandler(repoMock.Object);
        var result = await handler.Handle(
            new FindNearbyStoresQuery
            {
                UserLat = 10.78,
                UserLong = 106.69,
                RadiusKm = 3.5,
                Limit = 2
            },
            CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Sua Da Roastery");
        result[0].IsOpenNow.Should().BeTrue();
        result[1].IsOpenNow.Should().BeFalse();
    }
}
