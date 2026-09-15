using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Entities;

namespace Terminus.Application.Common.Rules;

public record CreateWaybillContext(
    Contract Contract, 
    int RequestedQuantity) : IRuleContext;

public record CreateContractContext(
    Consumer Consumer, 
    Product Product, 
    int Quantity) : IRuleContext;
    
public record SignContractContext(
    Contract Contract, 
    DateTime ConclusionDate) : IRuleContext;

public record DispatchWaybillContext(
    Contract Contract, 
    DateTime DispatchDate) : IRuleContext;