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

        builder.Property(x => x.Actors)
            .HasMaxLength(2000);

        builder.ToTable("Movies", t =>
        {
            t.HasCheckConstraint("CK_Movies_DurationMinutes", "[DurationMinutes] > 0");
        });

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

        builder.Property(x => x.TmdbRating)
       .HasPrecision(3, 1);

        builder.Property(x => x.RatingCount)
       .IsRequired()
       .HasDefaultValue(0);

        builder.Property(x => x.UserAverageRating)
            .HasPrecision(3, 1);

        builder.Property(x => x.TmdbId);

        builder.HasIndex(x => x.Title);

        builder.HasIndex(x => x.Slug)
            .IsUnique();
       
        builder.Property(x => x.ReleaseDate)
       .HasColumnType("date");

        builder.HasIndex(x => x.ReleaseDate);

        builder.HasIndex(x => x.TmdbId)
            .IsUnique()
            .HasFilter("[TmdbId] IS NOT NULL");

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x=>x.Language);

    }
}