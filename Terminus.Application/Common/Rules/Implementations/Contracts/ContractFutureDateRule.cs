using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Common.Rules.Implementations.Contracts;

public class ContractFutureDateRule(TimeProvider timeProvider) : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        SignContractContext ctx when ctx.ConclusionDate.Date > timeProvider.GetUtcNow().UtcDateTime.Date 
            => RuleResult.Failed("Дата підписання договору не може бути в майбутньому часі."),
            
        _ => RuleResult.Ok()
    };
}