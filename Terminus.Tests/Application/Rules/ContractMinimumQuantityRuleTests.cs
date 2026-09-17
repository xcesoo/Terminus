using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Contracts;
using Terminus.Domain.Entities;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Rules;

public class ContractMinimumQuantityRuleTests
{
    private readonly ContractMinimumQuantityRule _rule = new();

    // The terminal's minimum batch size (5 units total) must be enforced when a contract is created.
    [Fact]
    public void Apply_TotalQuantityBelowMinimum_Fails()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var items = new[] { new ContractItem(product.Id, 4) };

        var result = _rule.Apply(new CreateContractContext(consumer, items));

        Assert.False(result.IsSuccess);
        Assert.Contains("5 одиниць", result.ErrorMessage);
    }

    // Exactly the minimum quantity is accepted.
    [Fact]
    public void Apply_TotalQuantityEqualsMinimum_Succeeds()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var items = new[] { new ContractItem(product.Id, 5) };

        var result = _rule.Apply(new CreateContractContext(consumer, items));

        Assert.True(result.IsSuccess);
    }

    // The minimum applies to the sum across all line items, not each line individually.
    [Fact]
    public void Apply_TotalQuantityAcrossMultipleItemsMeetsMinimum_Succeeds()
    {
        var consumer = TestFactory.CreateConsumer();
        var productA = TestFactory.CreateProduct(code: "A");
        var productB = TestFactory.CreateProduct(code: "B");
        var items = new[] { new ContractItem(productA.Id, 2), new ContractItem(productB.Id, 3) };

        var result = _rule.Apply(new CreateContractContext(consumer, items));

        Assert.True(result.IsSuccess);
    }
}
