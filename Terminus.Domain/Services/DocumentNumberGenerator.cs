using Terminus.Domain.Interfaces;

namespace Terminus.Domain.Services;

public class DocumentNumberGenerator(TimeProvider timeProvider) : IDocumentNumberGenerator
{
    public string GenerateContractNumber()
    {
        return $"CTR-{GetDatePart()}-{GetRandomString(10)}";
    }

    public string GenerateWaybillNumber()
    {
        return $"WB-{GetDatePart()}-{GetRandomString(10)}";
    }

    private string GetDatePart() => timeProvider.GetUtcNow().ToString("yyyyMMddfff");
    
    private static string GetRandomString(int length) => 
        new(Enumerable.Range(0, length).Select(_ => (char)Random.Shared.Next('A', 'Z' + 1)).ToArray());
}