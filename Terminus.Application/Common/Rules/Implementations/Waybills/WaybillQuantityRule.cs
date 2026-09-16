using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Enums;

namespace Terminus.Application.Common.Rules.Implementations.Waybills;

public class WaybillQuantityRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context)
    {
        if (context is not CreateWaybillContext ctx) 
            return RuleResult.Ok();

        foreach (var reqItem in ctx.RequestedItems)
        {
            var contractItem = ctx.Contract.Items.FirstOrDefault(i => i.ProductId == reqItem.ProductId);
            
            if (contractItem == null)
                return RuleResult.Failed($"Товар з ID {reqItem.ProductId} відсутній у цьому договорі.");

            var alreadyShippedQty = ctx.Contract.Waybills
                .Where(w => w.Status != WaybillStatus.Cancelled)
                .SelectMany(w => w.Items)
                .Where(i => i.ProductId == reqItem.ProductId)
                .Sum(i => i.ShippedQuantity);

            // 3. Перевіряємо залишок
            if (reqItem.ShippedQuantity > contractItem.Quantity - alreadyShippedQty)
            {
                return RuleResult.Failed(
                    $"Неможливо відвантажити {reqItem.ShippedQuantity} шт. " +
                    $"Залишок за договором (разом із чернетками): {contractItem.Quantity - alreadyShippedQty} шт.");
            }
        }

        return RuleResult.Ok();
    }
}