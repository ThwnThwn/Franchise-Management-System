using FluentAssertions;
using SalesService.Application.Features.Orders.Commands.Create;

namespace SalesService.Application.Tests.Features.Orders.Commands;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_WhenItemsIsEmpty()
    {
        var cmd = new CreateOrderCommand(new CreateOrderRequestDto { Items = new List<CartItemDto>() });
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Items cannot be empty");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenItemSkuIsEmpty()
    {
        var cmd = new CreateOrderCommand(new CreateOrderRequestDto { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "", Quantity = 1 } } 
        });
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "SKU is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenItemQuantityIsZero()
    {
        var cmd = new CreateOrderCommand(new CreateOrderRequestDto { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU-001", Quantity = 0 } } 
        });
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Quantity must be greater than 0");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenValid()
    {
        var cmd = new CreateOrderCommand(new CreateOrderRequestDto { 
            Items = new List<CartItemDto> { new CartItemDto { Sku = "SKU-001", Quantity = 2 } } 
        });
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
