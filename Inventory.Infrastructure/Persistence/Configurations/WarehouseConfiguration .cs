using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouses");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.Location)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(w => w.ContactPerson)
                .HasMaxLength(100);

            builder.Property(w => w.ContactEmail)
                .HasMaxLength(100);

            builder.Property(w => w.ContactPhone)
                .HasMaxLength(20);

            builder.Property(w => w.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(w => w.CreatedAt)
                .IsRequired();

            builder.Property(w => w.UpdatedAt)
                .IsRequired();

            // Index for faster searches
            builder.HasIndex(w => w.Name);
            builder.HasIndex(w => w.Location);
        }
    }
}
