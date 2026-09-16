using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terminus.Domain.Entities;

namespace Terminus.Infrastructure.Persistence.Configurations;

public class WaybillConfiguration : IEntityTypeConfiguration<Waybill>
{
    public void Configure(EntityTypeBuilder<Waybill> builder)
    {
        builder.ToTable("waybills");

        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(w => w.Status)
            .HasColumnName("status")
            .HasConversion<string>() 
            .IsRequired();

        builder.Property(w => w.WaybillNumber)
            .HasColumnName("waybill_number")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.DispatchDate)
            .HasColumnName("dispatch_date")
            .HasDefaultValue(null);
        
        builder.OwnsMany(w => w.Items, ib =>
        {
            ib.ToTable("waybill_items");
    
            ib.WithOwner().HasForeignKey("waybill_id");
            ib.HasKey("waybill_id", nameof(WaybillItem.ProductId));

            ib.Property(i => i.ShippedQuantity).HasColumnName("shipped_quantity").IsRequired();
            ib.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();

            ib.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Property(w => w.ContractId)
            .HasColumnName("contract_id");

        builder.HasOne(w => w.Contract)
            .WithMany(c => c.Waybills)
            .HasForeignKey(w => w.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasDiscriminator<string>("transport_type")
            .HasValue<AutoWaybill>("auto")
            .HasValue<TrainWaybill>("train")
            .HasValue<AviaWaybill>("avia");
    }
}