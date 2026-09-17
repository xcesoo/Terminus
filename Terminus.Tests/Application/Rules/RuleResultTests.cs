using Terminus.Application.Common.Rules;

namespace Terminus.Tests.Application.Rules;

public class RuleResultTests
{
    // Ok() represents success with no error message.
    [Fact]
    public void Ok_HasSuccessTrueAndNoErrorMessage()
    {
        var result = RuleResult.Ok();

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }

    // Failed(message) represents failure and carries the given message.
    [Fact]
    public void Failed_HasSuccessFalseAndErrorMessage()
    {
        var result = RuleResult.Failed("boom");

        Assert.False(result.IsSuccess);
        Assert.Equal("boom", result.ErrorMessage);
    }

    // The implicit bool conversion lets callers write "if (result)" and must track IsSuccess.
    [Fact]
    public void ImplicitBoolConversion_ReflectsIsSuccess()
    {
        bool ok = RuleResult.Ok();
        bool failed = RuleResult.Failed("boom");

        Assert.True(ok);
        Assert.False(failed);
    }
}
