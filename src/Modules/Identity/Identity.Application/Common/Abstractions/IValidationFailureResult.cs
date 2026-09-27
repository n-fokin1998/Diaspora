namespace Diaspora.Identity.Application.Common.Abstractions;

public interface IValidationFailureResult<TSelf> where TSelf : IValidationFailureResult<TSelf>
{
    static abstract TSelf ValidationFailed(IReadOnlyDictionary<string, string[]> fieldErrors);
}
