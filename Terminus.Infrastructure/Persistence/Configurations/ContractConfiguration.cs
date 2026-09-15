using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terminus.Domain.Entities;

namespace Terminus.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(c => c.IsTerminated)
            .HasColumnName("is_terminated")
            .HasDefaultValue(false);

        builder.Property(c => c.ContractNumber)
            .HasColumnName("contract_number")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ConclusionDate)
            .HasColumnName("conclusion_date")
            .IsRequired();

        builder.Property(c => c.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(c => c.ConsumerId)
            .HasColumnName("consumer_id");

        builder.Property(c => c.ProductId)
            .HasColumnName("product_id");
        
        builder.HasOne(c => c.Consumer)
            .WithMany(c => c.Contracts)
            .HasForeignKey(c => c.ConsumerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Product)
            .WithMany(p => p.Contracts)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata
            .FindNavigation(nameof(Contract.Waybills))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}