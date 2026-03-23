using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ScreeningConfiguration : IEntityTypeConfiguration<Screening>
{
    public void Configure(EntityTypeBuilder<Screening> builder)
    {
        builder.ToTable("Screenings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SubtitleLanguage)
            .HasMaxLength(20);

        builder.Property(x => x.Format)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Movie)
            .WithMany(x=>x.Screenings) 
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Hall)
            .WithMany(x=>x.Screenings) 
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.HallId, x.StartTime });

        builder.HasIndex(x => x.MovieId);
    }
}