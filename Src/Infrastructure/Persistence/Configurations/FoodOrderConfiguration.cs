using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class FoodOrderConfiguration : IEntityTypeConfiguration<FoodOrder>
{
    public void Configure(EntityTypeBuilder<FoodOrder> builder)
    {
        builder.ToTable("FoodOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.DeliveryType)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.HasOne(x => x.SeatHold)
            .WithMany(x=>x.FoodOrders)
            .HasForeignKey(x => x.SeatHoldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Screening)
            .WithMany(x=>x.FoodOrders)
            .HasForeignKey(x => x.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Seat)
            .WithMany(x=>x.FoodOrders)
            .HasForeignKey(x => x.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.FoodOrderItems)
            .WithOne(x => x.FoodOrder)
            .HasForeignKey(x => x.FoodOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SeatHoldId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ScreeningId);
        builder.HasIndex(x => x.Status);
    }
}