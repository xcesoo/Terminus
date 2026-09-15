using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations.Contracts;

public class TerminatedContractRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        CreateWaybillContext ctx when ctx.Contract.IsTerminated 
            => RuleResult.Failed("Неможливо створити ТТН: договір розірвано."),
            
        _ => RuleResult.Ok()
    };
}