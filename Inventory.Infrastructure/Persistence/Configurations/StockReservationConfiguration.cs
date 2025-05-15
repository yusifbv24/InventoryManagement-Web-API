using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
    {
        public void Configure(EntityTypeBuilder<StockReservation> builder)
        {
            builder.ToTable("StockReservations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ProductId)
                .IsRequired();

            builder.Property(r => r.WarehouseId)
                .IsRequired();

            builder.Property(r => r.LocationCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Quantity)
                .IsRequired();

            builder.Property(r => r.OrderId)
                .IsRequired();

            builder.Property(r => r.ReservationDate)
                .IsRequired();

            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes for frequent query patterns
            builder.HasIndex(r => r.OrderId);
            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.IsActive);
            builder.HasIndex(r => r.ExpiryDate);
        }
    }
}
