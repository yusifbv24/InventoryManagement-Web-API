using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.Property(i => i.LocationCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.ReorderThreshold)
                .IsRequired();

            builder.Property(i => i.TargetStockLevel)
                .IsRequired();

            builder.Property(i => i.LastUpdated)
                .IsRequired();

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(i => i.Warehouse)
                .WithMany(w => w.InventoryItems)
                .HasForeignKey(i => i.WarehouseId);

            // Unique constraint for product, warehouse, and location
            builder.HasIndex(i => new { i.ProductId, i.WarehouseId, i.LocationCode })
                .IsUnique();
        }
    }
}
