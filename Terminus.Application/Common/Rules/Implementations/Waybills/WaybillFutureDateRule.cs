using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations.Waybills;

public class WaybillFutureDateRule(TimeProvider timeProvider) : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        DispatchWaybillContext ctx when ctx.DispatchDate.Date > timeProvider.GetUtcNow().UtcDateTime.Date 
            => RuleResult.Failed("Дата відвантаження ТТН не може бути в майбутньому часі."),
            
        _ => RuleResult.Ok()
    };
}