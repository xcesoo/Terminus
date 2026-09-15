using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations;

public class ContractMinimumQuantityRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        CreateContractContext ctx when ctx.Quantity < 5 
            => RuleResult.Failed("Мінімальна партія для укладання договору з терміналом - 5 одиниць виробу."),
            
        _ => RuleResult.Ok()
    };
}