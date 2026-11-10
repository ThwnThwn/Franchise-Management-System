using FluentValidation.TestHelper;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Features.Ingrdient.Commands.CreateIngredient;
using InventoryService.Application.Validators.Ingredient;
using Xunit;

namespace InventoryService.Application.Tests.Validators.Ingredient;

public class CreateIngredientCommandValidatorTests
{
    private readonly CreateIngredientCommandValidator _validator = new();

    [Fact]
    public void Should_Be_Valid_When_Using_Correct_Units()
    {
        var req = new CreateIngredientReq { Name = "Sữa", BaseUnit = BaseUnitConstants.Gram }; // Dùng "g"
        _validator.TestValidate(new CreateIngredientCommand(req)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Unit_Is_Invalid()
    {
        var req = new CreateIngredientReq { Name = "Sữa", BaseUnit = "Gram" }; // "Gram" là sai, phải là "g"
        _validator.TestValidate(new CreateIngredientCommand(req))
            .ShouldHaveValidationErrorFor(x => x.Request.BaseUnit)
            .WithErrorMessage(Message.Ingredient.InvalidBaseUnit);
    }
}