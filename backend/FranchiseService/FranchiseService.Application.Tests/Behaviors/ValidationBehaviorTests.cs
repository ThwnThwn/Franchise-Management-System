using FluentAssertions;
using FluentValidation;
using FranchiseService.Application.Behaviors;
using MediatR;

namespace FranchiseService.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnNextResult_WhenNoValidators()
    {
        var behavior = new ValidationBehavior<BrewCoffeeCommand, string>(new List<IValidator<BrewCoffeeCommand>>());

        var result = await behavior.Handle(
            new BrewCoffeeCommand { BlendName = "Arabica House" },
            () => Task.FromResult("ok"),
            CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenAnyValidatorFails()
    {
        var validators = new List<IValidator<BrewCoffeeCommand>>
        {
            new BrewCoffeeValidator(),
            new BrewSizeValidator()
        };

        var behavior = new ValidationBehavior<BrewCoffeeCommand, string>(validators);

        var act = () => behavior.Handle(
            new BrewCoffeeCommand { BlendName = string.Empty, CupSize = 0 },
            () => Task.FromResult("should-not-run"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldInvokeNext_WhenAllValidatorsPass()
    {
        var validators = new List<IValidator<BrewCoffeeCommand>>
        {
            new BrewCoffeeValidator(),
            new BrewSizeValidator()
        };

        var nextCalled = false;
        var behavior = new ValidationBehavior<BrewCoffeeCommand, string>(validators);

        var result = await behavior.Handle(
            new BrewCoffeeCommand { BlendName = "Robusta Milk", CupSize = 16 },
            () =>
            {
                nextCalled = true;
                return Task.FromResult("validated");
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("validated");
    }

    private class BrewCoffeeCommand : IRequest<string>
    {
        public string BlendName { get; set; } = string.Empty;
        public int CupSize { get; set; }
    }

    private class BrewCoffeeValidator : AbstractValidator<BrewCoffeeCommand>
    {
        public BrewCoffeeValidator()
        {
            RuleFor(x => x.BlendName).NotEmpty();
        }
    }

    private class BrewSizeValidator : AbstractValidator<BrewCoffeeCommand>
    {
        public BrewSizeValidator()
        {
            RuleFor(x => x.CupSize).GreaterThan(0);
        }
    }
}
