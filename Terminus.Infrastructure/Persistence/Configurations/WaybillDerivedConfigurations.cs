using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terminus.Domain.Entities;

namespace Terminus.Infrastructure.Persistence.Configurations;

public class AutoWaybillConfiguration : IEntityTypeConfiguration<AutoWaybill>
{
    public void Configure(EntityTypeBuilder<AutoWaybill> builder)
    {
        builder
            .Property(a => a.CarNumber)
            .HasColumnName("car_number")
            .HasMaxLength(50);
        
        builder
            .Property(a => a.RouteSheetNumber)
            .HasColumnName("route_sheet_number")
            .HasMaxLength(100);
        
        builder
            .Property(a => a.AutoServiceSum)
            .HasColumnName("auto_service_sum")
            .HasColumnType("numeric(18,2)");
    }
}

public class TrainWaybillConfiguration : IEntityTypeConfiguration<TrainWaybill>
{
    public void Configure(EntityTypeBuilder<TrainWaybill> builder)
    {
        builder.Property(t => t.ContainerNumber)
            .HasColumnName("container_number")
            .HasMaxLength(50);
        
        builder.Property(t => t.RailwayReceiptNumber)
            .HasColumnName("railway_receipt_number")
            .HasMaxLength(100);
        
        builder.Property(t => t.TrainServiceSum)
            .HasColumnName("train_service_sum")
            .HasColumnType("numeric(18,2)");
    }
}

public class AviaWaybillConfiguration : IEntityTypeConfiguration<AviaWaybill>
{
    public void Configure(EntityTypeBuilder<AviaWaybill> builder)
    {
        builder.Property(a => a.FlightNumber)
            .HasColumnName("flight_number")
            .HasMaxLength(50);
        
        builder.Property(a => a.AviaReceiptNumber)
            .HasColumnName("avia_receipt_number")
            .HasMaxLength(100);
        
        builder.Property(a => a.AviaServiceSum)
            .HasColumnName("avia_service_sum")
            .HasColumnType("numeric(18,2)");
    }
}