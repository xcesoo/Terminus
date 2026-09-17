using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Contracts;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Rules;

public class ContractSignStateRuleTests
{
    private readonly ContractSignStateRule _rule = new();

    private static Terminus.Domain.Entities.Contract CreateContract()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        return TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
    }

    // Signing an already-signed contract must be rejected — no re-signing.
    [Fact]
    public void Apply_ContractAlreadySigned_Fails()
    {
        var contract = CreateContract();
        contract.Sign(DateTime.UtcNow);

        var result = _rule.Apply(new SignContractContext(contract, DateTime.UtcNow));

        Assert.False(result.IsSuccess);
        Assert.Contains("вже підписано", result.ErrorMessage);
    }

    // A terminated contract can never be (re)signed.
    [Fact]
    public void Apply_ContractTerminated_Fails()
    {
        var contract = CreateContract();
        contract.Terminate();

        var result = _rule.Apply(new SignContractContext(contract, DateTime.UtcNow));

        Assert.False(result.IsSuccess);
        Assert.Contains("розірваний", result.ErrorMessage);
    }

    // A Draft contract is the only status that allows signing.
    [Fact]
    public void Apply_ContractInDraft_Succeeds()
    {
        var contract = CreateContract();

        var result = _rule.Apply(new SignContractContext(contract, DateTime.UtcNow));

        Assert.True(result.IsSuccess);
    }
}
