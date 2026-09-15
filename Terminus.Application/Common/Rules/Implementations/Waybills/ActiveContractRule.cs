using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Enums;

namespace Terminus.Application.Common.Rules.Implementations.Waybills;

public class ActiveContractRule : IBusinessRule
{
    public RuleResult Apply(IRuleContext context) => context switch
    {
        CreateWaybillContext ctx when ctx.Contract.Status == ContractStatus.Draft 
            => RuleResult.Failed("Неможливо створити ТТН: договір ще не підписано (статус Чернетка)."),
            
        CreateWaybillContext ctx when ctx.Contract.Status == ContractStatus.Terminated 
            => RuleResult.Failed("Неможливо створити ТТН: договір розірвано."),
            
        _ => RuleResult.Ok()
    };
}