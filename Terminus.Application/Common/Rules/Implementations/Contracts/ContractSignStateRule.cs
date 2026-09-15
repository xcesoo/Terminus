using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Enums;

namespace Terminus.Application.Common.Rules.Implementations.Contracts;

public class ContractSignStateRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        SignContractContext ctx when ctx.Contract.Status == ContractStatus.Signed 
            => RuleResult.Failed("Цей договір вже підписано. Повторне підписання заборонено."),
            
        SignContractContext ctx when ctx.Contract.Status == ContractStatus.Terminated 
            => RuleResult.Failed("Неможливо підписати розірваний договір."),
            
        _ => RuleResult.Ok()
    };
}