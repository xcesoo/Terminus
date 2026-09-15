using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terminus.Domain.Entities;

namespace Terminus.Infrastructure.Persistence.Configurations;

public class ConsumerConfiguration : IEntityTypeConfiguration<Consumer>
{
    public void Configure(EntityTypeBuilder<Consumer> builder)
    {
        builder.ToTable("consumers");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Address)
            .HasColumnName("address")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.BankAccount)
            .HasColumnName("bank_account")
            .IsRequired()
            .HasMaxLength(50);

        builder.Metadata
            .FindNavigation(nameof(Consumer.Contracts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}