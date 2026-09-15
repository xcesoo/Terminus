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

        builder.Property(w => w.WaybillNumber)
            .HasColumnName("waybill_number")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.DispatchDate)
            .HasColumnName("dispatch_date")
            .IsRequired();

        builder.Property(w => w.ShippedQuantity)
            .HasColumnName("shipped_quantity")
            .IsRequired();

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