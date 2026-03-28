using FluentAssertions;
using FluentValidation.TestHelper;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Ingrdient.Commands.DeleteIngredient;
using InventoryService.Application.Validators.Ingredient;

namespace InventoryService.Application.Tests.Validators.Ingredient;

public class DeleteIngredientCommandValidatorTests
{
    private readonly DeleteIngredientCommandValidator _validator;

    public DeleteIngredientCommandValidatorTests()
    {
        _validator = new DeleteIngredientCommandValidator();
    }

    // 1. Phủ xanh Happy Path: Guid hợp lệ
    [Fact]
    public void Should_Not_Have_Error_When_Guid_Is_Valid()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var command = new DeleteIngredientCommand(validId);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();

        // Test thêm Property của Record để phủ 100% Get
        command.Request.Should().Be(validId);
    }

    // 2. Phủ nhánh: Guid bị rỗng (Guid.Empty)
    [Fact]
    public void Should_Have_Error_When_Guid_Is_Empty()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var command = new DeleteIngredientCommand(emptyId);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request)
              .WithErrorMessage(Message.Ingredient.Required);
    }
}