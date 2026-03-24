using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SeatHoldConfiguration : IEntityTypeConfiguration<SeatHold>
{
    public void Configure(EntityTypeBuilder<SeatHold> builder)
    {
        builder.ToTable("SeatHolds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(x => x.Screening)
            .WithMany(x => x.SeatHolds)
            .HasForeignKey(x => x.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Seat)
            .WithMany(x => x.SeatHolds)
            .HasForeignKey(x => x.SeatId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(x => new { x.ScreeningId, x.SeatId });
    }
}