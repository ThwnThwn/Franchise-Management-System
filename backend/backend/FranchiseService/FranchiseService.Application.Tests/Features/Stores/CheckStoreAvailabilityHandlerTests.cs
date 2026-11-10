using FluentAssertions;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.Stores.Queries.CheckStoreAvailability;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using Moq;

namespace FranchiseService.Application.Tests.Features.Stores;

public class CheckStoreAvailabilityHandlerTests
{
    [Fact]
    public async Task Handle_ShouldThrow_WhenStoreIdIsInvalidGuid()
    {
        var repoMock = new Mock<IStoreRepository>();
        var handler = new CheckStoreAvailabilityHandler(repoMock.Object);

        var act = () => handler.Handle(new CheckStoreAvailabilityQuery { StoreId = "invalid-guid" }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*ID không hợp lệ*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenStoreNotFound()
    {
        var storeId = Guid.NewGuid();
        var repoMock = new Mock<IStoreRepository>();

        repoMock
            .Setup(x => x.GetStoreWithHoursAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        var handler = new CheckStoreAvailabilityHandler(repoMock.Object);

        var act = () => handler.Handle(new CheckStoreAvailabilityQuery { StoreId = storeId.ToString() }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnUnavailable_WhenStoreIsNotOpenStatus()
    {
        var storeId = Guid.NewGuid();
        var repoMock = new Mock<IStoreRepository>();

        repoMock
            .Setup(x => x.GetStoreWithHoursAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Store
            {
                Id = storeId,
                Name = "Arabica Closed",
                Status = (int)StoreStatus.Closed,
                StoreHours = new List<StoreHour>()
            });

        var handler = new CheckStoreAvailabilityHandler(repoMock.Object);
        var result = await handler.Handle(new CheckStoreAvailabilityQuery { StoreId = storeId.ToString() }, CancellationToken.None);

        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("không mở");
    }

    [Fact]
    public async Task Handle_ShouldReturnUnavailable_WhenTodayIsClosed()
    {
        var storeId = Guid.NewGuid();
        var vnNow = GetVnNow();
        var day = (int)vnNow.DayOfWeek;

        var store = new Store
        {
            Id = storeId,
            Name = "Robusta Weekend",
            Status = (int)StoreStatus.Open,
            StoreHours =
            [
                new StoreHour
                {
                    Id = Guid.NewGuid(),
                    StoreId = storeId,
                    DayOfWeek = day,
                    OpenTime = new TimeOnly(8, 0),
                    CloseTime = new TimeOnly(22, 0),
                    IsClosed = true,
                    Store = null!
                }
            ]
        };

        var repoMock = new Mock<IStoreRepository>();
        repoMock
            .Setup(x => x.GetStoreWithHoursAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        var handler = new CheckStoreAvailabilityHandler(repoMock.Object);
        var result = await handler.Handle(new CheckStoreAvailabilityQuery { StoreId = storeId.ToString() }, CancellationToken.None);

        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("nghỉ");
    }

    [Fact]
    public async Task Handle_ShouldReturnUnavailable_WhenOutOfServingHours()
    {
        var storeId = Guid.NewGuid();
        var vnNow = GetVnNow();
        var currentTime = TimeOnly.FromDateTime(vnNow);
        var day = (int)vnNow.DayOfWeek;

        var store = new Store
        {
            Id = storeId,
            Name = "Sua Da Late Night",
            Status = (int)StoreStatus.Open,
            StoreHours =
            [
                new StoreHour
                {
                    Id = Guid.NewGuid(),
                    StoreId = storeId,
                    DayOfWeek = day,
                    OpenTime = currentTime.AddMinutes(1),
                    CloseTime = currentTime.AddMinutes(2),
                    IsClosed = false,
                    Store = null!
                }
            ]
        };

        var repoMock = new Mock<IStoreRepository>();
        repoMock
            .Setup(x => x.GetStoreWithHoursAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        var handler = new CheckStoreAvailabilityHandler(repoMock.Object);
        var result = await handler.Handle(new CheckStoreAvailabilityQuery { StoreId = storeId.ToString() }, CancellationToken.None);

        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("hết giờ");
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailable_WhenStoreIsOpenNow()
    {
        var storeId = Guid.NewGuid();
        var vnNow = GetVnNow();

        var store = new Store
        {
            Id = storeId,
            Name = "Arabica Open All Day",
            Status = (int)StoreStatus.Open,
            StoreHours =
            [
                new StoreHour
                {
                    Id = Guid.NewGuid(),
                    StoreId = storeId,
                    DayOfWeek = (int)vnNow.DayOfWeek,
                    OpenTime = new TimeOnly(0, 0),
                    CloseTime = new TimeOnly(23, 59),
                    IsClosed = false,
                    Store = null!
                }
            ]
        };

        var repoMock = new Mock<IStoreRepository>();
        repoMock
            .Setup(x => x.GetStoreWithHoursAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        var handler = new CheckStoreAvailabilityHandler(repoMock.Object);
        var result = await handler.Handle(new CheckStoreAvailabilityQuery { StoreId = storeId.ToString() }, CancellationToken.None);

        result.IsAvailable.Should().BeTrue();
        result.Message.Should().BeNull();
    }

    private static DateTime GetVnNow()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }
}


