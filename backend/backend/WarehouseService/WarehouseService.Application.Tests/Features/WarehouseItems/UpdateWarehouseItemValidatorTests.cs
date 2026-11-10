namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseItems.Command.UpdateWarehouseItem;

public class UpdateWarehouseItemValidatorTests
{
    private readonly UpdateWarehouseItemCommandValidator _validator;

    public UpdateWarehouseItemValidatorTests()
    {
        _validator = new UpdateWarehouseItemCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = CreateCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse item ID must not be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenUnitIsEmpty(string? invalidUnit)
    {
        var command = CreateCommand(unit: invalidUnit!);

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
    public void Should_HaveError_WhenBasePriceIsNegative()
    {
        var command = CreateCommand(basePrice: -5000);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BasePrice)
            .WithErrorMessage("Base price must be greater than or equal to 0.");
    }

    [Fact]
    public void Should_NotHaveError_WhenBasePriceIsExactlyZero()
    {
        // Test kiểm tra biên (boundary) để đảm bảo >= 0 hoạt động chuẩn
        var command = CreateCommand(basePrice: 0);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.BasePrice);
    }

    [Fact]
    public void Should_HaveError_WhenExpiryDateIsInThePast()
    {
        // HasValue == true nhưng ngày lại là hôm qua
        var command = CreateCommand(expiryDate: DateTime.UtcNow.AddDays(-1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryDate)
            .WithErrorMessage("Expiry date must be in the future.");
    }

    [Fact]
    public void Should_NotHaveError_WhenExpiryDateIsNull()
    {
        // Phủ xanh nhánh .When(x => x.ExpiryDate.HasValue) == false (Vật tư không có HSD)
        var command = CreateCommand(expiryDate: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Cập nhật thông tin Syrup Vanilla Monin
        var validCommand = new UpdateWarehouseItemCommand(
            Id: Guid.NewGuid(),
            Unit: "Chai 700ml",
            BasePrice: 185000,
            ExpiryDate: DateTime.UtcNow.AddYears(1)
        );

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper tạo Command nhanh để tránh lặp code, gom tham số vào một chỗ
    private static UpdateWarehouseItemCommand CreateCommand(
        Guid? id = null,
        string unit = "Kg",
        decimal basePrice = 120000,
        DateTime? expiryDate = null)
    {
        return new UpdateWarehouseItemCommand(
            Id: id ?? Guid.NewGuid(),
            Unit: unit,
            BasePrice: basePrice,
            // Mặc định tương lai nếu không có yêu cầu đặc biệt từ test case
            ExpiryDate: expiryDate ?? DateTime.UtcNow.AddMonths(6)
        );
    }
}