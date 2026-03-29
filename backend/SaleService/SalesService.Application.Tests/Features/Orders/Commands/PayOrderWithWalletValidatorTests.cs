using FluentAssertions;
using SalesService.Application.Features.Orders.Commands.PayWithWallet;

namespace SalesService.Application.Tests.Features.Orders.Commands;

public class PayOrderWithWalletValidatorTests
{
    private readonly PayOrderWithWalletValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_WhenOrderIdIsEmpty()
    {
        var cmd = new PayOrderWithWalletCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "OrderId is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCustomerIdIsEmpty()
    {
        var cmd = new PayOrderWithWalletCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "CustomerId is required");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenValid()
    {
        var cmd = new PayOrderWithWalletCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
