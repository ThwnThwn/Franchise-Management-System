namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockOut;

public class CreateStockOutValidatorTests
{
    private readonly CreateStockOutCommandValidator _validator;

    public CreateStockOutValidatorTests()
    {
        _validator = new CreateStockOutCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenWarehouseIdIsEmpty()
    {
        var command = CreateCommand(warehouseId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.WarehouseId)
            .WithErrorMessage("Warehouse ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenItemsAreEmpty()
    {
        var command = CreateCommand(items: []);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Stock out must contain at least one item.");
    }

    [Fact]
    public void Should_HaveError_WhenItemsExceed100()
    {
        var tooManyItems = Enumerable.Range(1, 101)
            .Select(_ => new StockOutItemDto(Guid.NewGuid(), 10, 50000))
            .ToList();

        var command = CreateCommand(items: tooManyItems);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Stock out cannot contain more than 100 items.");
    }

    [Fact]
    public void Should_HaveError_WhenIngredientIdIsEmpty()
    {
        var invalidItem = new StockOutItemDto(Guid.Empty, 5, 100000);
        var command = CreateCommand(items: [invalidItem]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].IngredientId")
            .WithErrorMessage("Ingredient ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenQuantityIsZeroOrLess()
    {
        var zeroQuantityItem = new StockOutItemDto(Guid.NewGuid(), 0, 100000);
        var negativeQuantityItem = new StockOutItemDto(Guid.NewGuid(), -5, 100000);

        var commandZero = CreateCommand(items: [zeroQuantityItem]);
        var commandNegative = CreateCommand(items: [negativeQuantityItem]);

        _validator.TestValidate(commandZero)
            .ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than 0.");

        _validator.TestValidate(commandNegative)
            .ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than 0.");
    }

    [Fact]
    public void Should_HaveError_WhenUnitPriceIsNegative()
    {
        var invalidItem = new StockOutItemDto(Guid.NewGuid(), 10, -500);
        var command = CreateCommand(items: [invalidItem]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice")
            .WithErrorMessage("Unit price must be greater than or equal to 0.");
    }

    [Fact]
    public void Should_HaveError_WhenCreatedByIsEmpty()
    {
        var command = CreateCommand(createdBy: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CreatedBy)
            .WithErrorMessage("CreatedBy must not be empty.");
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        // Kịch bản xuất kho 50kg Hạt Arabica với giá 250,000đ
        var validItem = new StockOutItemDto(Guid.NewGuid(), 50, 250000);
        var command = CreateCommand(items: [validItem]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Hàm Helper để dễ dàng tạo Command với các tham số mặc định hợp lệ
    private static CreateStockOutCommand CreateCommand(
        Guid? warehouseId = null,
        List<StockOutItemDto>? items = null,
        Guid? createdBy = null)
    {
        return new CreateStockOutCommand(
            WarehouseId: warehouseId ?? Guid.NewGuid(),
            Items: items ?? [new StockOutItemDto(Guid.NewGuid(), 10, 150000)],
            CreatedBy: createdBy ?? Guid.NewGuid(),
            Note: null
        );
    }
}