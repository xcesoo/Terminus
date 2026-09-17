using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Waybills;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Rules;

public class WaybillChronologyRuleTests
{
    private readonly WaybillChronologyRule _rule = new();

    private static Terminus.Domain.Entities.Contract CreateSignedContract(DateTime conclusionDate)
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
        contract.Sign(conclusionDate);
        return contract;
    }

    // Dispatch is impossible before the underlying contract has a conclusion date at all.
    [Fact]
    public void Apply_ContractNotSigned_Fails()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
        var context = new DispatchWaybillContext(contract, DateTime.UtcNow);

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("не підписано", result.ErrorMessage);
    }

    // A shipment cannot be dated before the contract that authorizes it was even signed.
    [Fact]
    public void Apply_DispatchBeforeConclusionDate_Fails()
    {
        var contract = CreateSignedContract(new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc));
        var context = new DispatchWaybillContext(contract, new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc));

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("ранішою", result.ErrorMessage);
    }

    // Dispatching on the exact same day the contract was signed is the earliest allowed boundary.
    [Fact]
    public void Apply_DispatchOnConclusionDate_Succeeds()
    {
        var date = new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc);
        var contract = CreateSignedContract(date);
        var context = new DispatchWaybillContext(contract, date);

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }

    // Any dispatch date after the conclusion date is valid.
    [Fact]
    public void Apply_DispatchAfterConclusionDate_Succeeds()
    {
        var contract = CreateSignedContract(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        var context = new DispatchWaybillContext(contract, new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc));

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }
}
