using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ProductId)
                .IsRequired();

            builder.Property(t => t.LocationCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.Quantity)
                .IsRequired();

            builder.Property(t => t.Timestamp)
                .IsRequired();

            builder.Property(t => t.ReferenceNumber)
                .HasMaxLength(50);

            builder.Property(t => t.Notes)
                .HasMaxLength(500);

            builder.Property(t => t.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            // Relationships
            builder.HasOne(t => t.Warehouse)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WarehouseId);

            // Indexes for frequent query patterns
            builder.HasIndex(t => t.ProductId);
            builder.HasIndex(t => t.WarehouseId);
            builder.HasIndex(t => t.Timestamp);
            builder.HasIndex(t => t.ReferenceNumber);
        }
    }
}
