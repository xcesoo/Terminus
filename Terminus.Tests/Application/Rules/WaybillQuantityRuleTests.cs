using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Waybills;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.ValueObjects;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Rules;

public class WaybillQuantityRuleTests
{
    private readonly WaybillQuantityRule _rule = new();

    // You can only ship products that actually appear on the underlying contract.
    [Fact]
    public void Apply_ProductNotInContract_Fails()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        var otherProductId = Guid.NewGuid();

        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(otherProductId, 1) });

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("відсутній у цьому договорі", result.ErrorMessage);
    }

    // Requesting exactly the full remaining contract quantity is allowed.
    [Fact]
    public void Apply_RequestedQuantityWithinLimit_Succeeds()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });

        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(product.Id, 10) });

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }

    // Requesting more than the contract ever ordered must be rejected.
    [Fact]
    public void Apply_RequestedQuantityExceedsContractLimit_Fails()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });

        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(product.Id, 11) });

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("Залишок за договором", result.ErrorMessage);
    }

    // Quantity already shipped on a prior waybill against the same contract reduces what's left to ship.
    [Fact]
    public void Apply_AlreadyShippedByOtherWaybill_ReducesRemainingLimit()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });

        var existingWaybill = AutoWaybill.Create(
            "WB-0001", contract.Id, "KA0000KA", "PL-1",
            new DeliveryCostBreakdown(0m, 0m, 1m),
            new[] { TestFactory.CreateWaybillItem(product, 7) });
        TestFactory.LinkWaybillToContract(existingWaybill, contract);

        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(product.Id, 4) });

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("3 шт", result.ErrorMessage);
    }

    // A cancelled waybill's quantity is given back to the contract's remaining balance.
    [Fact]
    public void Apply_CancelledWaybillQuantity_IsNotCountedAgainstLimit()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });

        var cancelledWaybill = AutoWaybill.Create(
            "WB-0001", contract.Id, "KA0000KA", "PL-1",
            new DeliveryCostBreakdown(0m, 0m, 1m),
            new[] { TestFactory.CreateWaybillItem(product, 7) });
        cancelledWaybill.Cancel();
        TestFactory.LinkWaybillToContract(cancelledWaybill, contract);

        var context = new CreateWaybillContext(contract, new[] { new WaybillItemRequestDto(product.Id, 10) });

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }

    // The rule must no-op for contexts it doesn't understand, so RuleEngine can run it unconditionally.
    [Fact]
    public void Apply_UnrelatedContext_ReturnsOk()
    {
        var consumer = TestFactory.CreateConsumer();
        var contract = TestFactory.CreateContract(consumer, Array.Empty<ContractItem>());

        var result = _rule.Apply(new SignContractContext(contract, DateTime.UtcNow));

        Assert.True(result.IsSuccess);
    }
}
