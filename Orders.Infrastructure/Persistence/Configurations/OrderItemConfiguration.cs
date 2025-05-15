using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Ordes.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.ProductSKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.IsReserved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.ReservationNotes)
                .HasMaxLength(500);

            // Index for faster lookup
            builder.HasIndex(i => i.ProductId);
        }
    }
}
