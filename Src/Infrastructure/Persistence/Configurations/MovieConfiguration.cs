using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        builder.Property(x => x.AgeRating)
            .HasMaxLength(20);

        builder.Property(x => x.Tagline)
            .HasMaxLength(300);

        builder.Property(x => x.Director)
            .HasMaxLength(150);

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(50);

        builder.Property(x => x.ImdbRating)
            .HasPrecision(3, 1);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.TmdbId);

        builder.HasIndex(x => x.Title);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => x.ReleaseDate);

        builder.HasIndex(x => x.TmdbId)
            .IsUnique()
            .HasFilter("[TmdbId] IS NOT NULL");

        builder.HasIndex(x => x.Status);

    }
}