using System.Text.RegularExpressions;
using Terminus.Domain.Services;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Domain.Services;

public class DocumentNumberGeneratorTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 9, 17, 10, 30, 45, 123, TimeSpan.Zero);

    // Contract numbers follow "CTR-<date+ms>-<10 random letters>".
    [Fact]
    public void GenerateContractNumber_HasExpectedFormat()
    {
        var generator = new DocumentNumberGenerator(new FixedTimeProvider(FixedNow));

        var number = generator.GenerateContractNumber();

        // "yyyyMMddfff" is date + millisecond only, not the full time-of-day.
        Assert.Matches(new Regex(@"^CTR-20260917123-[A-Z]{10}$"), number);
    }

    // Waybill numbers follow "WB-<date+ms>-<10 random letters>".
    [Fact]
    public void GenerateWaybillNumber_HasExpectedFormat()
    {
        var generator = new DocumentNumberGenerator(new FixedTimeProvider(FixedNow));

        var number = generator.GenerateWaybillNumber();

        Assert.Matches(new Regex(@"^WB-20260917123-[A-Z]{10}$"), number);
    }

    // The random suffix must differ between calls even when the clock is frozen, or numbers would collide.
    [Fact]
    public void GenerateWaybillNumber_ConsecutiveCalls_ProduceDifferentNumbers()
    {
        var generator = new DocumentNumberGenerator(new FixedTimeProvider(FixedNow));

        var first = generator.GenerateWaybillNumber();
        var second = generator.GenerateWaybillNumber();

        Assert.NotEqual(first, second);
    }
}
