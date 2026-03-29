using FluentValidation.TestHelper;
using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using Xunit;
using System;

namespace SalesService.Application.Tests.Features.Reports;

public class GetRevenueReportQueryValidatorTest
{
    private readonly GetRevenueReportQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_FromDate_Greater_Than_ToDate()
    {
        var from = new DateOnly(2024, 1, 2);
        var to = new DateOnly(2024, 1, 1);
        var query = new GetRevenueReportQuery(from, to, null, null);
        
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var from = new DateOnly(2024, 1, 1);
        var to = new DateOnly(2024, 1, 2);
        var query = new GetRevenueReportQuery(from, to, null, null);
        
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
