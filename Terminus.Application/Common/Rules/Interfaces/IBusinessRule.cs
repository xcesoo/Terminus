namespace Terminus.Application.Common.Rules.Interfaces;

public interface IBusinessRule
{
    RuleResult Apply(IRuleContext context);
}