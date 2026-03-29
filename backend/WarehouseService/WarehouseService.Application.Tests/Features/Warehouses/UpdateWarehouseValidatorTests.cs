namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.Warehouses.Command.UpdateWarehouse;
using WarehouseService.Domain.Enums;

public class UpdateWarehouseValidatorTests
{
    private readonly UpdateWarehouseCommandValidator _validator;

    public UpdateWarehouseValidatorTests()
    {
        _validator = new UpdateWarehouseCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = CreateCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenCodeExceeds50Characters()
    {
        var longCode = new string('A', 51); // 51 ký tự
        var command = CreateCommand(code: longCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Warehouse code must not exceed 50 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_NotHaveError_WhenCodeIsNullOrWhitespace(string? invalidCode)
    {
        // Phủ nhánh .When(x => !string.IsNullOrWhiteSpace(x.Code)) == false
        var command = CreateCommand(code: invalidCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Should_HaveError_WhenNameExceeds255Characters()
    {
        var longName = new string('N', 256); // 256 ký tự
        var command = CreateCommand(name: longName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Warehouse name must not exceed 255 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_NotHaveError_WhenNameIsNullOrWhitespace(string? invalidName)
    {
        // Phủ nhánh .When(x => !string.IsNullOrWhiteSpace(x.Name)) == false
        var command = CreateCommand(name: invalidName);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_WhenStatusIsInvalid()
    {
        var command = CreateCommand(status: (WarehouseStatus)999);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Warehouse status is invalid.");
    }

    [Fact]
    public void Should_NotHaveError_WhenStatusIsNull()
    {
        // Phủ nhánh .When(x => x.Status.HasValue) == false
        var command = CreateCommand(status: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_HaveError_WhenStoreIdIsEmpty()
    {
        // HasValue == true nhưng Value == Guid.Empty
        var command = CreateCommand(storeId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StoreId)
            .WithErrorMessage("Store ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveError_WhenStoreIdIsNull()
    {
        // Phủ nhánh .When(x => x.StoreId.HasValue) == false
        var command = CreateCommand(storeId: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.StoreId);
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Cập nhật "Tổng Kho Arabica"
        var command = new UpdateWarehouseCommand(
            Id: Guid.NewGuid(),
            Code: "WH-ARABICA-VIP",
            Name: "Tổng Kho Arabica Tầng 1",
            Address: "Đà Lạt, Lâm Đồng",
            Status: WarehouseStatus.Active,
            StoreId: Guid.NewGuid()
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper tạo Command nhanh để tránh lặp code, gom tham số vào một chỗ
    private static UpdateWarehouseCommand CreateCommand(
        Guid? id = null,
        string? code = "WH-ROBUSTA",
        string? name = "Kho Robusta Sài Gòn",
        string? address = "Quận 1, HCM",
        WarehouseStatus? status = WarehouseStatus.Active,
        Guid? storeId = null)
    {
        // Gán default Guid hợp lệ nếu truyền null,
        // riêng storeId để mặc định null vì logic update thường cho phép giữ nguyên store cũ
        return new UpdateWarehouseCommand(
            Id: id ?? Guid.NewGuid(),
            Code: code,
            Name: name,
            Address: address,
            Status: status,
            StoreId: storeId == Guid.Empty ? Guid.Empty : storeId ?? Guid.NewGuid()
        );
    }
}