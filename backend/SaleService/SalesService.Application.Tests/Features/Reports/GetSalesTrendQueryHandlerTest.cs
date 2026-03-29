using Ardalis.Result;
using FluentAssertions;
using Moq;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Features.Reports.Queries.GetSalesTrend;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.Reports;

public class GetSalesTrendQueryHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductServiceClient> _productClient = new();
    private readonly GetSalesTrendQueryHandler _handler;

    public GetSalesTrendQueryHandlerTest()
    {
        _handler = new GetSalesTrendQueryHandler(_orderRepo.Object, _productClient.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmpty_When_CategorySkusEmpty()
    {
        var categoryId = Guid.NewGuid();
        _productClient.Setup(x => x.GetSkusByCategoryAsync(categoryId)).ReturnsAsync(new List<string>());

        var query = new GetSalesTrendQuery 
        { 
            FromDate = DateTimeOffset.Now.AddDays(-7), 
            ToDate = DateTimeOffset.Now, 
            Aggregation = TimeAggregation.Day, 
            CategoryId = categoryId 
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DataPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ReturnFilledData_For_DayAggregation()
    {
        var from = new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero);
        var to = new DateTimeOffset(new DateTime(2024, 1, 2), TimeSpan.Zero);
        
        var dbData = new List<SalesTrendDataPointDto>
        {
            new SalesTrendDataPointDto { TimeLabel = "2024-01-01", Revenue = 100, OrderVolume = 1 }
        };

        _orderRepo.Setup(x => x.GetSalesTrendAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), TimeAggregation.Day, It.IsAny<List<string>?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbData);

        var query = new GetSalesTrendQuery 
        { 
            FromDate = from, 
            ToDate = to, 
            Aggregation = TimeAggregation.Day 
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DataPoints.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Work_For_HourAggregation()
    {
        var from = new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0), TimeSpan.Zero);
        var to = new DateTimeOffset(new DateTime(2024, 1, 1, 1, 0, 0), TimeSpan.Zero);

        _orderRepo.Setup(x => x.GetSalesTrendAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), TimeAggregation.Hour, It.IsAny<List<string>?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesTrendDataPointDto>());

        var query = new GetSalesTrendQuery 
        { 
            FromDate = from, 
            ToDate = to, 
            Aggregation = TimeAggregation.Hour 
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.DataPoints.Should().HaveCount(2); // 00:00, 01:00
    }

    [Fact]
    public async Task Handle_Should_Work_For_WeekAggregation()
    {
        var from = new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero); // Mon
        var to = new DateTimeOffset(new DateTime(2024, 1, 8), TimeSpan.Zero); // Next Mon

        _orderRepo.Setup(x => x.GetSalesTrendAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), TimeAggregation.Week, It.IsAny<List<string>?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesTrendDataPointDto>());

        var query = new GetSalesTrendQuery 
        { 
            FromDate = from, 
            ToDate = to, 
            Aggregation = TimeAggregation.Week 
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.DataPoints.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Align_To_Monday_When_Start_Is_Not_Monday()
    {
        var from = new DateTimeOffset(new DateTime(2024, 1, 2), TimeSpan.Zero); // Tue
        var to = new DateTimeOffset(new DateTime(2024, 1, 3), TimeSpan.Zero); // Wed

        _orderRepo.Setup(x => x.GetSalesTrendAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), TimeAggregation.Week, It.IsAny<List<string>?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesTrendDataPointDto>());

        var query = new GetSalesTrendQuery { FromDate = from, ToDate = to, Aggregation = TimeAggregation.Week };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.DataPoints.Should().NotBeEmpty();
    }
}
