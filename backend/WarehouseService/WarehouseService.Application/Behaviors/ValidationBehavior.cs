using FluentValidation;
using MediatR;

namespace WarehouseService.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        //Validate
        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        //Collect failures
        var failures = results
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .ToList();

        //Throw if invalid
        if (failures.Count != 0)
            throw new ValidationException(failures);

        //Continue pipeline
        return await next();
    }
}
