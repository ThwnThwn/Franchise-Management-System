using FluentValidation.TestHelper;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Category;
using InventoryService.Application.Features.Category.Commands.UpdateCategory;
using InventoryService.Application.Validators.Category;
using Xunit;

namespace InventoryService.Application.Tests.Validators.Category;

public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var req = new UpdateCategoryRequest { Name = new string('A', 101) };
        var result = _validator.TestValidate(new UpdateCategoryCommand(req));

        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage(Message.Category.NameTooLong);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Null()
    {
        var req = new UpdateCategoryRequest { Name = null }; // .When sẽ trả về false -> Bỏ qua check
        var result = _validator.TestValidate(new UpdateCategoryCommand(req));
        result.ShouldNotHaveAnyValidationErrors();
    }
}