using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Waybills;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Rules;

public class WaybillFutureDateRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 17, 12, 0, 0, TimeSpan.Zero);

    private readonly WaybillFutureDateRule _rule = new(new FixedTimeProvider(Now));

    private static Terminus.Domain.Entities.Contract CreateContract()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        return TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
    }

    // You cannot dispatch a shipment "in the future" relative to the server clock.
    [Fact]
    public void Apply_DispatchDateInFuture_Fails()
    {
        var context = new DispatchWaybillContext(CreateContract(), Now.UtcDateTime.AddDays(1));

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("майбутньому", result.ErrorMessage);
    }

    // Today is the latest allowed dispatch date.
    [Fact]
    public void Apply_DispatchDateToday_Succeeds()
    {
        var context = new DispatchWaybillContext(CreateContract(), Now.UtcDateTime);

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }

    // Backdating a dispatch into the past is allowed by this rule (chronology vs. contract date is a separate rule).
    [Fact]
    public void Apply_DispatchDateInPast_Succeeds()
    {
        var context = new DispatchWaybillContext(CreateContract(), Now.UtcDateTime.AddDays(-5));

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }
}
