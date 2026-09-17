namespace Terminus.Infrastructure.Configuration;

public class DeliveryPricingOptions
{
    public const string SectionName = "DeliveryPricing";

    public decimal BasePricePerItem { get; set; } = 100m;
    public decimal CommissionRate { get; set; } = 0.02m;
    public decimal AutoMultiplier { get; set; } = 1.0m;
    public decimal TrainMultiplier { get; set; } = 1.5m;
    public decimal AviaMultiplier { get; set; } = 3.0m;
}
