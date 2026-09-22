using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Application.Common.Behaviors;
using MediatR;

namespace Diaspora.Tests.Modules.Identity.Application.Common.Behaviors;

public class ValidationBehaviorTests
{
    private sealed record TestRequest(string Value) : IRequest<TestResponse>;

    private sealed class TestResponse : IValidationFailureResult<TestResponse>
    {
        public bool Succeeded { get; private init; }
        public IReadOnlyDictionary<string, string[]> FieldErrors { get; private init; } = new Dictionary<string, string[]>();

        public static TestResponse Success() => new() { Succeeded = true };

        public static TestResponse ValidationFailed(IReadOnlyDictionary<string, string[]> fieldErrors) => new()
        {
            Succeeded = false,
            FieldErrors = fieldErrors,
        };
    }

    private sealed class FieldValidator(string field, string message) : IValidator<TestRequest>
    {
        public IReadOnlyDictionary<string, string[]> Validate(TestRequest request) =>
            request.Value == field
                ? new Dictionary<string, string[]> { [field] = [message] }
                : new Dictionary<string, string[]>();
    }

    [Fact]
    public async Task Handle_WithNoValidationErrors_InvokesNext()
    {
        var sut = new ValidationBehavior<TestRequest, TestResponse>([]);
        var nextCalled = false;

        var result = await sut.Handle(new TestRequest("anything"), _ =>
        {
            nextCalled = true;
            return Task.FromResult(TestResponse.Success());
        }, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShortCircuitsWithoutInvokingNext()
    {
        var sut = new ValidationBehavior<TestRequest, TestResponse>([new FieldValidator("email", "Email is required.")]);
        var nextCalled = false;

        var result = await sut.Handle(new TestRequest("email"), _ =>
        {
            nextCalled = true;
            return Task.FromResult(TestResponse.Success());
        }, CancellationToken.None);

        Assert.False(nextCalled);
        Assert.False(result.Succeeded);
        Assert.Contains("email", result.FieldErrors.Keys);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_MergesFieldErrors()
    {
        var sut = new ValidationBehavior<TestRequest, TestResponse>(
        [
            new FieldValidator("both", "First error."),
            new AlwaysFailsValidator("both", "Second error."),
        ]);

        var result = await sut.Handle(new TestRequest("both"), _ => Task.FromResult(TestResponse.Success()), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["First error.", "Second error."], result.FieldErrors["both"]);
    }

    private sealed class AlwaysFailsValidator(string field, string message) : IValidator<TestRequest>
    {
        public IReadOnlyDictionary<string, string[]> Validate(TestRequest request) =>
            new Dictionary<string, string[]> { [field] = [message] };
    }
}
