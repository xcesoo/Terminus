using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations;

public class CompletedContractLockRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        CreateWaybillContext ctx when ctx.Contract.Waybills.Sum(w => w.ShippedQuantity) >= ctx.Contract.Quantity 
            => RuleResult.Failed("Цей договір вже повністю виконаний. Створення нових ТТН заборонено."),
            
        _ => RuleResult.Ok()
    };
}