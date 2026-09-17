using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Tests.Application.Rules;

public class RuleEngineTests
{
    private class FakeContext : IRuleContext;

    private class AlwaysOkRule : IBusinessRule
    {
        public bool WasCalled { get; private set; }

        public RuleResult Apply(IRuleContext context)
        {
            WasCalled = true;
            return RuleResult.Ok();
        }
    }

    private class AlwaysFailsRule(string message) : IBusinessRule
    {
        public bool WasCalled { get; private set; }

        public RuleResult Apply(IRuleContext context)
        {
            WasCalled = true;
            return RuleResult.Failed(message);
        }
    }

    // When every registered rule passes, Verify must report overall success and invoke all of them.
    [Fact]
    public void Verify_AllRulesPass_ReturnsOk()
    {
        var rule1 = new AlwaysOkRule();
        var rule2 = new AlwaysOkRule();
        var engine = new RuleEngine(new IBusinessRule[] { rule1, rule2 });

        var result = engine.Verify(new FakeContext());

        Assert.True(result.IsSuccess);
        Assert.True(rule1.WasCalled);
        Assert.True(rule2.WasCalled);
    }

    // The engine must stop at the first failing rule (short-circuit) and never invoke the rules after it.
    [Fact]
    public void Verify_FirstRuleFails_ReturnsFailureAndStopsShortCircuiting()
    {
        var failingRule = new AlwaysFailsRule("first failure");
        var neverReachedRule = new AlwaysOkRule();
        var engine = new RuleEngine(new IBusinessRule[] { failingRule, neverReachedRule });

        var result = engine.Verify(new FakeContext());

        Assert.False(result.IsSuccess);
        Assert.Equal("first failure", result.ErrorMessage);
        Assert.False(neverReachedRule.WasCalled);
    }

    // With no rules registered at all, verification trivially succeeds.
    [Fact]
    public void Verify_NoRules_ReturnsOk()
    {
        var engine = new RuleEngine(Array.Empty<IBusinessRule>());

        var result = engine.Verify(new FakeContext());

        Assert.True(result.IsSuccess);
    }
}
