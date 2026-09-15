using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations;

public class WaybillChronologyRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        CreateWaybillContext ctx when ctx.DispatchDate.Date < ctx.Contract.ConclusionDate.Date 
            => RuleResult.Failed("Дата відвантаження ТТН не може бути ранішою за дату укладання договору."),
            
        _ => RuleResult.Ok()
    };
}