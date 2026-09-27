using Diaspora.Identity.Application.Common.Abstractions;
using MediatR;

namespace Diaspora.Identity.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IValidationFailureResult<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var validator in validators)
        {
            foreach (var (field, messages) in validator.Validate(request))
            {
                errors[field] = errors.TryGetValue(field, out var existing) ? [.. existing, .. messages] : messages;
            }
        }

        if (errors.Count > 0)
        {
            return TResponse.ValidationFailed(errors);
        }

        return await next();
    }
}
