namespace Diaspora.Identity.Application.Common.Abstractions;

public interface IValidator<in TRequest>
{
    IReadOnlyDictionary<string, string[]> Validate(TRequest request);
}
