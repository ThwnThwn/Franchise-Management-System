using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using SalesService.Application.Features.Reports.Queries.ProductPerformance;
using SalesService.Application.Features.Reports.Queries.CustomerPurchaseBehavior;
using SalesService.Application.Features.Reports.Queries.GetSalesTrend;
using SalesService.Domain.Enums;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;
using System;
using System.Text;

namespace SalesService.Application.Tests.Features.Reports;

public class CsvExportersTests
{
    [Fact]
    public void RevenueReportCsvExporter_ShouldExport_WithStoreColumn_WhenStoreIdIsNull()
    {
        var report = new RevenueReportResponse
        {
            Items = new List<RevenueItemDto>
            {
                new RevenueItemDto 
                { 
                    StoreId = Guid.NewGuid(),
                    ProductName = "Product with , comma", 
                    Sku = "SKU\"Q", 
                    QuantitySold = 10, 
                    UnitPrice = 100, 
                    TotalRevenue = 1000 
                }
            },
            OverallTotalRevenue = 1000,
            StoreId = null
        };
 
        var bytes = RevenueReportCsvExporter.Export(report);
        var csv = Encoding.UTF8.GetString(bytes);
 
        csv.Should().Contain("StoreId,ProductName,Sku");
        csv.Should().Contain("\"Product with , comma\"");
        csv.Should().Contain("\"SKU\"\"Q\"");
        csv.Should().Contain("OverallTotal,1000");
    }

    [Fact]
    public void RevenueReportCsvExporter_ShouldExport_WithoutStoreColumn_WhenStoreIdIsProvided()
    {
        var report = new RevenueReportResponse
        {
            Items = new List<RevenueItemDto>
            {
                new RevenueItemDto 
                { 
                    ProductName = "P1", 
                    Sku = "S1", 
                    QuantitySold = 1, 
                    UnitPrice = 10, 
                    TotalRevenue = 10 
                }
            },
            OverallTotalRevenue = 10,
            StoreId = Guid.NewGuid()
        };

        var bytes = RevenueReportCsvExporter.Export(report);
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().NotContain("StoreId");
        csv.Should().Contain("ProductName,Sku");
    }

    [Fact]
    public void ProductPerformanceCsvExporter_ShouldExport_WhenDataExists()
    {
        var items = new List<ProductPerformanceResponse>
        {
            new ProductPerformanceResponse 
            { 
                ProductId = Guid.NewGuid(),
                ProductName = "P1", 
                CategoryName = "C1", 
                UnitsSold = 5, 
                Revenue = 500,
                Tag = PerformanceTag.Top
            }
        };

        var bytes = ProductPerformanceCsvExporter.Export(items);
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("P1");
        csv.Should().Contain("Top");
    }

    [Fact]
    public void CustomerOrderPatternCsvExporter_ShouldExport()
    {
        var items = new List<CustomerOrderPatternResponse>
        {
            new CustomerOrderPatternResponse 
            { 
                CustomerId = Guid.NewGuid(),
                FullName = "User 1", 
                TotalOrders = 5, 
                TotalSpent = 5000 
            }
        };

        var bytes = CustomerOrderPatternCsvExporter.Export(items);
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("User 1");
    }

    [Fact]
    public void SalesTrendCsvExporter_ShouldExport()
    {
        var response = new SalesTrendResponse
        {
            DataPoints = new List<SalesTrendDataPointDto>
            {
                new SalesTrendDataPointDto { TimeLabel = "2024-01-01", Revenue = 1000, OrderVolume = 10 }
            }
        };

        var bytes = SalesTrendCsvExporter.Export(response);
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("2024-01-01");
    }
}
