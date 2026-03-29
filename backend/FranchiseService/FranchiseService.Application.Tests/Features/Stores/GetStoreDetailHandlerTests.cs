using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Stores.Queries.GetStoreDetail;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Stores;

public class GetStoreDetailHandlerTests
{
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly GetStoreDetailHandler _handler;

    public GetStoreDetailHandlerTests()
    {
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _handler = new GetStoreDetailHandler(_storeRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenIdIsInvalidGuid()
    {
        var result = await _handler.Handle(new GetStoreDetailQuery { Id = "robusta-invalid" }, CancellationToken.None);

        result.Should().BeNull();
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenStoreNotFound()
    {
        var storeId = Guid.NewGuid();
        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        var result = await _handler.Handle(new GetStoreDetailQuery { Id = storeId.ToString() }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapStoreAndSetIsOpenNow_WhenStoreIsOpen()
    {
        var currentDay = (int)DateTime.UtcNow.DayOfWeek;
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CH-ROBUSTA-03",
            Name = "Quan Robusta Pho Hue",
            Address = "303 Pho Hue",
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

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(store.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        var result = await _handler.Handle(new GetStoreDetailQuery { Id = store.Id.ToString() }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Quan Robusta Pho Hue");
        result.IsOpenNow.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetIsOpenNowFalse_WhenStoreClosedBySchedule()
    {
        var currentDay = (int)DateTime.UtcNow.DayOfWeek;
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CH-SUADAC-04",
            Name = "Quan Sua Dac Ho Con Rua",
            Address = "404 Pham Ngoc Thach",
            Status = (int)StoreStatus.Open,
            StoreHours = new List<StoreHour>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = currentDay,
                    IsClosed = true,
                    OpenTime = new TimeOnly(0, 0),
                    CloseTime = new TimeOnly(23, 59)
                }
            },
            Shifts = new List<Shift>()
        };

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(store.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        var result = await _handler.Handle(new GetStoreDetailQuery { Id = store.Id.ToString() }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsOpenNow.Should().BeFalse();
    }
}
