using Terminus.Application.DTOs;

namespace Terminus.Application.Common.Rules.Interfaces;

public interface IContractPdfGenerator
{
    byte[] Generate(ContractDto contract, ConsumerDto consumer);
}
