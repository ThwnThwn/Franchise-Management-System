namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.Warehouses.Queries.GetAll;
using WarehouseService.Domain.Enums;

public class GetWarehousesValidatorTests
{
    private readonly GetWarehousesQueryValidator _validator;

    public GetWarehousesValidatorTests()
    {
        _validator = new GetWarehousesQueryValidator();
    }

    [Fact]
    public void Should_HaveError_WhenPageIsZeroOrLess()
    {
        var query = CreateQuery(page: 0);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page number must be greater than 0.");
    }

    [Fact]
    public void Should_HaveError_WhenPageSizeIsInvalid()
    {
        var queryUnder = CreateQuery(pageSize: 0);
        var queryOver = CreateQuery(pageSize: 101);

        var resultUnder = _validator.TestValidate(queryUnder);
        var resultOver = _validator.TestValidate(queryOver);

        resultUnder.ShouldHaveValidationErrorFor(x => x.PageSize);
        resultOver.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");
    }

    [Fact]
    public void Should_HaveError_WhenStatusIsInvalid()
    {
        var query = CreateQuery(status: (WarehouseStatus)999);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Warehouse status is invalid.");
    }

    [Fact]
    public void Should_NotHaveError_WhenStatusIsNull()
    {
        // Phủ xanh nhánh .When(x => x.Status.HasValue) == false
        var query = CreateQuery(status: null);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_HaveError_WhenSearchTermExceeds100Characters()
    {
        var longTerm = new string('S', 101); // 101 ký tự
        var query = CreateQuery(searchTerm: longTerm);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
            .WithErrorMessage("Search term must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_NotHaveError_WhenSearchTermIsNullOrWhitespace(string? invalidTerm)
    {
        // Phủ xanh nhánh .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm)) == false
        var query = CreateQuery(searchTerm: invalidTerm);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenQueryIsValid()
    {
        // Kịch bản truy vấn hợp lệ: Tìm các kho cà phê đang hoạt động ở trang 2
        var query = new GetWarehousesQuery(
            Page: 2,
            PageSize: 20,
            Status: WarehouseStatus.Active,
            SearchTerm: "Kho Arabica",
            StoreId: null
        );

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số, giúp các Test Case ngắn gọn và dễ đọc
    private static GetWarehousesQuery CreateQuery(
        int page = 1,
        int pageSize = 10,
        WarehouseStatus? status = WarehouseStatus.Active,
        string? searchTerm = "Robusta")
    {
        return new GetWarehousesQuery(
            Page: page,
            PageSize: pageSize,
            Status: status,
            SearchTerm: searchTerm,
            StoreId: null
        );
    }
}