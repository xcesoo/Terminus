using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Entities;

namespace Terminus.Application.Common.Rules;

public record CreateWaybillContext(
    Contract Contract, 
    int RequestedQuantity, 
    DateTime DispatchDate) : IRuleContext;

public record CreateContractContext(
    Consumer Consumer, 
    Product Product, 
    int Quantity, 
    DateTime ConclusionDate) : IRuleContext;