using FluentValidation.TestHelper;
using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Features.Roles.Commands.UpdateRole;
using IdentityService.Application.Validators.Roles;
using Xunit;

namespace IdentityService.Application.Tests.Features.Roles;

public class UpdateRoleHandlerTest
{
    private readonly UpdateRoleCommandValidator _validator;

    public UpdateRoleHandlerTest()
    {
        _validator = new UpdateRoleCommandValidator();
    }

    // 1. VALIDATION THÀNH CÔNG
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var request = new UpdateRoleReq { Name = "Admin" };
        var command = new UpdateRoleCommand(Guid.NewGuid(), request);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // 2. KIỂM TRA ID (NotEmpty)
    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.Empty, new UpdateRoleReq { Name = "Admin" });

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Role Id is required");
    }

    // 3. KIỂM TRA NAME (NotEmpty)
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Have_Error_When_Name_Is_Empty(string? invalidName)
    {
        // Arrange
        var request = new UpdateRoleReq { Name = invalidName! };
        var command = new UpdateRoleCommand(Guid.NewGuid(), request);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Role name is required");
    }

    // 4. KIỂM TRA NAME (MaximumLength)
    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var longName = new string('a', 51); // 51 ký tự (vượt mức 50)
        var request = new UpdateRoleReq { Name = longName };
        var command = new UpdateRoleCommand(Guid.NewGuid(), request);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Request.Name);
    }
}