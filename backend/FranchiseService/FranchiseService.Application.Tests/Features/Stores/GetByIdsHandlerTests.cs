using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Stores.Queries.GetByIds;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Stores;

public class GetByIdsHandlerTests
{
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly GetByIdsHandler _handler;

    public GetByIdsHandlerTests()
    {
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _handler = new GetByIdsHandler(_storeRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenAllIdsInvalid()
    {
        var query = new GetByIdsQuery { Ids = new[] { "arabica", "robusta" } };

        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        result.Should().BeEmpty();
        _storeRepositoryMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenRepositoryReturnsNoStore()
    {
        var validId = Guid.NewGuid();
        _storeRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Store>());

        var result = (await _handler.Handle(new GetByIdsQuery { Ids = new[] { validId.ToString() } }, CancellationToken.None)).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapStoresAndCalculateIsOpenNow()
    {
        var currentDay = (int)DateTime.UtcNow.DayOfWeek;
        var openStore = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CH-ARABICA-01",
            Name = "Quan Arabica Cua Dong",
            Address = "101 Dong Khoi",
            Phone = "0909111222",
            Latitude = 10.77m,
            Longitude = 106.70m,
            Status = (int)StoreStatus.Open,
            StoreHours = new List<StoreHour>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = currentDay,
                    IsClosed = false,
                    OpenTime = new TimeOnly(0, 0),
                    CloseTime = new TimeOnly(23, 59)
                }
            },
            Shifts = new List<Shift>()
        };

        var closedStore = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CH-SUADAC-02",
            Name = "Quan Sua Dac Cho Ben Thanh",
            Address = "202 Le Thanh Ton",
            Status = (int)StoreStatus.Closed,
            StoreHours = new List<StoreHour>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = currentDay,
                    IsClosed = false,
                    OpenTime = new TimeOnly(0, 0),
                    CloseTime = new TimeOnly(23, 59)
                }
            },
            Shifts = new List<Shift>()
        };

        _storeRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { openStore, closedStore });

        var query = new GetByIdsQuery { Ids = new[] { openStore.Id.ToString(), closedStore.Id.ToString() } };
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == openStore.Id && s.IsOpenNow);
        result.Should().Contain(s => s.Id == closedStore.Id && !s.IsOpenNow);
    }
}
