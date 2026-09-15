namespace Terminus.Application.Common.Rules.Interfaces;

public interface IRuleEngine
{
    RuleResult Verify(IRuleContext context);
}