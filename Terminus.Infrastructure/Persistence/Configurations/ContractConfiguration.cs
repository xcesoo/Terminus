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
        
        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>() 
            .IsRequired();

        builder.Property(c => c.ContractNumber)
            .HasColumnName("contract_number")
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(c => c.ContractNumber)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(c => c.ConclusionDate)
            .HasColumnName("conclusion_date")
            .HasDefaultValue(null);
        

        builder.Property(c => c.ConsumerId)
            .HasColumnName("consumer_id");
        
        
        builder.HasOne(c => c.Consumer)
            .WithMany(c => c.Contracts)
            .HasForeignKey(c => c.ConsumerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.OwnsMany(c => c.Items, ib =>
        {
            ib.ToTable("contract_items"); 
    
            ib.WithOwner().HasForeignKey("contract_id");
    
            ib.HasKey("contract_id", nameof(ContractItem.ProductId));

            ib.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
            ib.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();

            ib.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Metadata
            .FindNavigation(nameof(Contract.Waybills))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}