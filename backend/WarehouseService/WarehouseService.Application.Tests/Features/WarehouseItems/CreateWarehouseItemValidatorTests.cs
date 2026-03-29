namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;

public class CreateWarehouseItemValidatorTests
{
    private readonly CreateWarehouseItemCommandValidator _validator;

    public CreateWarehouseItemValidatorTests()
    {
        _validator = new CreateWarehouseItemCommandValidator();
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
    public void Should_HaveError_WhenIngredientIdIsEmpty()
    {
        var command = CreateCommand(ingredientId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IngredientId)
            .WithErrorMessage("Ingredient ID must not be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenUnitIsEmpty(string? invalidUnit)
    {
        var command = CreateCommand(unit: invalidUnit!); // Force null để test

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage("Unit must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenUnitExceeds50Characters()
    {
        var longUnit = new string('U', 51); // 51 ký tự
        var command = CreateCommand(unit: longUnit);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage("Unit must not exceed 50 characters.");
    }

    [Fact]
    public void Should_HaveError_WhenQuantityIsNegative()
    {
        var command = CreateCommand(quantity: -1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than or equal to 0.");
    }

    [Fact]
    public void Should_HaveError_WhenBasePriceIsNegative()
    {
        var command = CreateCommand(basePrice: -15000);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BasePrice)
            .WithErrorMessage("Base price must be greater than or equal to 0.");
    }

    [Fact]
    public void Should_HaveError_WhenExpiryDateIsInThePast()
    {
        // HasValue == true nhưng là ngày hôm qua
        var command = CreateCommand(expiryDate: DateTime.UtcNow.AddDays(-1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryDate)
            .WithErrorMessage("Expiry date must be in the future.");
    }

    [Fact]
    public void Should_NotHaveError_WhenExpiryDateIsNull()
    {
        // Phủ xanh nhánh .When(x => x.ExpiryDate.HasValue) == false
        var command = CreateCommand(expiryDate: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Nhập 50kg Hạt Moka, giá 120k, HSD năm sau.
        // Lưu ý Quantity = 0 và BasePrice = 0 vẫn là hợp lệ theo rule (>= 0)
        var validCommand = new CreateWarehouseItemCommand(
            WarehouseId: Guid.NewGuid(),
            IngredientId: Guid.NewGuid(),
            Unit: "Kg",
            Quantity: 50,
            BasePrice: 120000,
            ExpiryDate: DateTime.UtcNow.AddYears(1)
        );

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenQuantityAndBasePriceAreExactlyZero()
    {
        // Test boundary (đường biên) để chắc chắn >= 0 hoạt động chuẩn
        var command = CreateCommand(quantity: 0, basePrice: 0);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
        result.ShouldNotHaveValidationErrorFor(x => x.BasePrice);
    }

    // Helper method gom tham số, giúp các Test Case luôn ngắn gọn, sạch đẹp
    private static CreateWarehouseItemCommand CreateCommand(
        Guid? warehouseId = null,
        Guid? ingredientId = null,
        string unit = "Hộp",
        decimal quantity = 10,
        decimal basePrice = 50000,
        DateTime? expiryDate = null)
    {
        // Mặc định trả về ExpiryDate tương lai nếu không ai truyền vào
        return new CreateWarehouseItemCommand(
            WarehouseId: warehouseId ?? Guid.NewGuid(),
            IngredientId: ingredientId ?? Guid.NewGuid(),
            Unit: unit,
            Quantity: quantity,
            BasePrice: basePrice,
            ExpiryDate: expiryDate ?? DateTime.UtcNow.AddMonths(3)
        );
    }
}