using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Contracts;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Rules;

public class ContractFutureDateRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 17, 12, 0, 0, TimeSpan.Zero);

    private readonly ContractFutureDateRule _rule = new(new FixedTimeProvider(Now));

    private static Terminus.Domain.Entities.Contract CreateContract()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        return TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
    }

    // A contract cannot be signed with a conclusion date that hasn't happened yet.
    [Fact]
    public void Apply_ConclusionDateInFuture_Fails()
    {
        var context = new SignContractContext(CreateContract(), Now.UtcDateTime.AddDays(1));

        var result = _rule.Apply(context);

        Assert.False(result.IsSuccess);
    }

    // Signing dated exactly today is the latest allowed boundary.
    [Fact]
    public void Apply_ConclusionDateToday_Succeeds()
    {
        var context = new SignContractContext(CreateContract(), Now.UtcDateTime);

        var result = _rule.Apply(context);

        Assert.True(result.IsSuccess);
    }
}
