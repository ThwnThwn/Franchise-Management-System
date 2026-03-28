using FluentValidation.TestHelper;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Category;
using InventoryService.Application.Features.Category.Commands.CreateCategory;
using InventoryService.Application.Validators.Category;

namespace InventoryService.Application.Tests.Validators.Category;

public class CreateCategoryCommandValidatorTests
{
    private readonly Application.Validators.Category.CreateCategoryCommandValidator _validator;

    public CreateCategoryCommandValidatorTests()
    {
        _validator = new CreateCategoryCommandValidator();
    }

    // 1. Phủ xanh Happy Path: Tên hợp lệ
    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "Cà phê máy" };
        var command = new CreateCategoryCommand(request);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // 2. Phủ nhánh: Tên trống (NotEmpty)
    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "" };
        var command = new CreateCategoryCommand(request);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage(Message.Category.NameRequired);
    }

    // 3. Phủ nhánh: Tên quá dài (MaximumLength - 100)
    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        // Arrange
        var longName = new string('A', 101); // 101 ký tự
        var request = new CreateCategoryRequest { Name = longName };
        var command = new CreateCategoryCommand(request);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage(Message.Category.NameTooLong);
    }
}