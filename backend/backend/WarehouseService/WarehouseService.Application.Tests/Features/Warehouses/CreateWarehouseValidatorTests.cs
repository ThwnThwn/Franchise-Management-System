namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.Warehouses.Command.CreateWarehouse;
using WarehouseService.Domain.Enums;

public class CreateWarehouseValidatorTests
{
    private readonly CreateWarehouseCommandValidator _validator;

    public CreateWarehouseValidatorTests()
    {
        _validator = new CreateWarehouseCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenCodeIsEmpty(string? invalidCode)
    {
        var command = CreateCommand(code: invalidCode!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Warehouse code must not be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenNameIsEmpty(string? invalidName)
    {
        var command = CreateCommand(name: invalidName!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Warehouse name must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenNameExceeds255Characters()
    {
        var longName = new string('N', 256);
        var command = CreateCommand(name: longName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Warehouse name must not exceed 255 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenAddressIsEmpty(string? invalidAddress)
    {
        var command = CreateCommand(address: invalidAddress!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage("Warehouse address must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenStoreIdIsEmpty()
    {
        var command = CreateCommand(storeId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StoreId)
            .WithErrorMessage("StoreId must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenStatusIsInvalid()
    {
        var command = CreateCommand(status: (WarehouseStatus)999);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid warehouse status.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Mở thêm kho cà phê mới
        var command = new CreateWarehouseCommand(
            Code: "WH-ARABICA-GL",
            Name: "Tổng Kho Arabica Gia Lai",
            Address: "Thành phố Pleiku, Gia Lai",
            StoreId: Guid.NewGuid(),
            Status: WarehouseStatus.Active
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Hàm Helper tạo Command hợp lệ mặc định, giúp các Test Case rút gọn code
    private static CreateWarehouseCommand CreateCommand(
        string code = "WH-ROBUSTA",
        string name = "Kho Robusta Sài Gòn",
        string address = "Quận 1, TP.HCM",
        Guid? storeId = null,
        WarehouseStatus status = WarehouseStatus.Active)
    {
        return new CreateWarehouseCommand(
            Code: code,
            Name: name,
            Address: address,
            StoreId: storeId ?? Guid.NewGuid(),
            Status: status
        );
    }
}