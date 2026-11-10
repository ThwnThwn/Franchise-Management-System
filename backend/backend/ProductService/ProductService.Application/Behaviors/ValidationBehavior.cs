using FluentValidation;
using MediatR;

namespace ProductService.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        //Validate
        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
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