namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseItems.Query.GetAll;

public class GetWarehouseItemsValidatorTests
{
    private readonly GetWarehouseItemsQueryValidator _validator;

    public GetWarehouseItemsValidatorTests()
    {
        _validator = new GetWarehouseItemsQueryValidator();
    }

    [Fact]
    public void Should_HaveError_WhenWarehouseIdIsEmpty()
    {
        var query = CreateQuery(warehouseId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.WarehouseId)
            .WithErrorMessage("Warehouse ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenPageIsZeroOrLess()
    {
        var queryZero = CreateQuery(page: 0);
        var queryNegative = CreateQuery(page: -1);

        _validator.TestValidate(queryZero)
            .ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page number must be greater than 0.");

        _validator.TestValidate(queryNegative)
            .ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page number must be greater than 0.");
    }

    [Fact]
    public void Should_HaveError_WhenPageSizeIsInvalid()
    {
        var queryUnder = CreateQuery(pageSize: 0);
        var queryOver = CreateQuery(pageSize: 101);

        _validator.TestValidate(queryUnder)
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");

        _validator.TestValidate(queryOver)
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenQueryIsValid()
    {
        // Kịch bản truy vấn hợp lệ: Lấy danh sách nguyên liệu của kho ở trang 2, mỗi trang 50 món
        var validQuery = new GetWarehouseItemsQuery(
            WarehouseId: Guid.NewGuid(),
            Page: 2,
            PageSize: 50,
            IngredientId: null,
            LowStock: null,
            ExpiringSoon: null
        );

        var result = _validator.TestValidate(validQuery);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số, giúp các Test Case luôn ngắn gọn, tránh lặp code
    private static GetWarehouseItemsQuery CreateQuery(
        Guid? warehouseId = null,
        int page = 1,
        int pageSize = 10)
    {
        return new GetWarehouseItemsQuery(
            WarehouseId: warehouseId ?? Guid.NewGuid(),
            Page: page,
            PageSize: pageSize,
            IngredientId: null,
            LowStock: null,
            ExpiringSoon: null
        );
    }
}