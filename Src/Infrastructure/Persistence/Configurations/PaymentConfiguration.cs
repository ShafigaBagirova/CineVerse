using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SeatHoldId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TicketAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(x => x.FoodAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ProviderPaymentIntentId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ClientSecret)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.PaidAtUtc)
            .IsRequired(false);

        builder.HasOne(x => x.SeatHold)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.SeatHoldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProviderPaymentIntentId).IsUnique();
        builder.HasIndex(x => x.SeatHoldId)
       .HasDatabaseName("UX_Payments_SeatHoldId_Pending")
       .IsUnique()
       .HasFilter($"[{nameof(Payment.Status)}] = {(int)PaymentStatus.Pending}");
        builder.Property(x => x.Currency)
            .HasConversion<int>()
            .IsRequired();
    }
}