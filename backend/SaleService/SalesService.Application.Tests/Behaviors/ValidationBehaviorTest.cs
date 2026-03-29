using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using SalesService.Application.Behaviors;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Behaviors;

public class ValidationBehaviorTest
{
    public class SampleRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_Should_CallNext_When_NoValidators()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(Enumerable.Empty<IValidator<SampleRequest>>());
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("ok"); };

        await behavior.Handle(new SampleRequest(), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_ValidationFails()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Prop", "Error") }));

        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validator.Object });
        
        Func<Task> act = () => behavior.Handle(new SampleRequest(), () => Task.FromResult("ok"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_Should_CallNext_When_ValidationPasses()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validator.Object });
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("ok"); };

        await behavior.Handle(new SampleRequest(), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }
}
