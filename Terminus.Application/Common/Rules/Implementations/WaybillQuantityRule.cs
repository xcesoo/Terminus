using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations;

public class WaybillQuantityRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        CreateWaybillContext ctx when ctx.RequestedQuantity <= 0 
            => RuleResult.Failed("Кількість відвантаження має бути більшою за нуль."),
            
        CreateWaybillContext ctx when ctx.RequestedQuantity > (ctx.Contract.Quantity - ctx.Contract.Waybills.Sum(w => w.ShippedQuantity)) 
            => RuleResult.Failed($"Неможливо відвантажити {ctx.RequestedQuantity} шт. Залишок по договору: {ctx.Contract.Quantity - ctx.Contract.Waybills.Sum(w => w.ShippedQuantity)} шт."),
            
        _ => RuleResult.Ok()
    };
}