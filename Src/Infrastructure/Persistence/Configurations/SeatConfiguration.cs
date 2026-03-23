using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Row)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Number)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Hall)
            .WithMany(x => x.Seats)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.HallId, x.Row, x.Number })
            .IsUnique();
    }
}