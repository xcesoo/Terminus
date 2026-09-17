using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Waybills;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Rules;

public class ActiveContractRuleTests
{
    private readonly ActiveContractRule _rule = new();

    private static Contract CreateContractWithStatus(Action<Contract> transition)
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
        transition(contract);
        return contract;
    }

    // A ТТН cannot be created against a contract that hasn't been signed yet.
    [Fact]
    public void Apply_ContractInDraft_Fails()
    {
        var contract = CreateContractWithStatus(_ => { });
        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(Guid.NewGuid(), 1) });

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("не підписано", result.ErrorMessage);
    }

    // A ТТН cannot be created against a terminated contract.
    [Fact]
    public void Apply_ContractTerminated_Fails()
    {
        var contract = CreateContractWithStatus(c => c.Terminate());
        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(Guid.NewGuid(), 1) });

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("розірвано", result.ErrorMessage);
    }

    // A signed contract is the only status that allows creating a waybill against it.
    [Fact]
    public void Apply_ContractSigned_Succeeds()
    {
        var contract = CreateContractWithStatus(c => c.Sign(DateTime.UtcNow));
        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(Guid.NewGuid(), 1) });

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }

    // The rule must no-op (not throw, not fail) for contexts it doesn't understand, so RuleEngine can run it unconditionally.
    [Fact]
    public void Apply_UnrelatedContext_ReturnsOk()
    {
        var result = _rule.Apply(new SignContractContext(CreateContractWithStatus(_ => { }), DateTime.UtcNow));

        Assert.True(result.IsSuccess);
    }
}
