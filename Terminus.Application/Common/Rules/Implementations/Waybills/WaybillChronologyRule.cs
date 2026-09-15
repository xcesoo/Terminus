using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations.Waybills;

public class WaybillChronologyRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        DispatchWaybillContext ctx when !ctx.Contract.ConclusionDate.HasValue 
            => RuleResult.Failed("Неможливо відвантажити ТТН: договір ще не підписано (відсутня дата укладання)."),
        
        DispatchWaybillContext ctx when ctx.Contract.ConclusionDate.HasValue && 
                                        ctx.DispatchDate.Date < ctx.Contract.ConclusionDate.Value.Date 
            => RuleResult.Failed("Дата відвантаження ТТН не може бути ранішою за дату підписання договору."),
            
        _ => RuleResult.Ok()
    };
}