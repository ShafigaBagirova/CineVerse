using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class FoodOrderItemConfiguration : IEntityTypeConfiguration<FoodOrderItem>
{
    public void Configure(EntityTypeBuilder<FoodOrderItem> builder)
    {
        builder.ToTable("FoodOrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(x => x.FoodOrder)
            .WithMany(x => x.FoodOrderItems)
            .HasForeignKey(x => x.FoodOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FoodItem)
            .WithMany(x => x.FoodOrderItems)
            .HasForeignKey(x => x.FoodItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}