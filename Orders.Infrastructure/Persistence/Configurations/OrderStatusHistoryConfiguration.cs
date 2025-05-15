using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Ordes.Infrastructure.Persistence.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistory");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.PreviousStatus)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(h => h.NewStatus)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(h => h.ChangedAt)
                .IsRequired();

            builder.Property(h => h.Notes)
                .HasMaxLength(1000);

            // Indexes
            builder.HasIndex(h => h.ChangedAt);
        }
    }
}
