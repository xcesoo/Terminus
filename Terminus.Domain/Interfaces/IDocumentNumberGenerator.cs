namespace Terminus.Domain.Interfaces;

public interface IDocumentNumberGenerator
{
    string GenerateContractNumber();
    string GenerateWaybillNumber();
}