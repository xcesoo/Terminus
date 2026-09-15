using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Enums;

namespace Terminus.Application.Common.Rules.Implementations.Waybills;

public class WaybillQuantityRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        CreateWaybillContext ctx when ctx.RequestedQuantity <= 0 
            => RuleResult.Failed("Кількість відвантаження має бути більшою за нуль."),
            
        CreateWaybillContext ctx when ctx.RequestedQuantity > (ctx.Contract.Quantity - ctx.Contract.Waybills.Where(w => w.Status != WaybillStatus.Cancelled).Sum(w => w.ShippedQuantity)) 
            => RuleResult.Failed($"Неможливо відвантажити {ctx.RequestedQuantity} шт. " +
                                 $"Залишок (разом із чернетками): {ctx.Contract.Quantity - ctx.Contract.Waybills.Where(w => w.Status != WaybillStatus.Cancelled).Sum(w => w.ShippedQuantity)} шт."),
            
        _ => RuleResult.Ok()
    };
}