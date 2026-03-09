using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class MoviePosterConfiguration : IEntityTypeConfiguration<MoviePoster>
{
    public void Configure(EntityTypeBuilder<MoviePoster> builder)
    {
        builder.ToTable("MoviePosters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ObjectKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Order)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.MovieId)
            .IsRequired();

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.MediaItems)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MovieId);

        builder.HasIndex(x => new { x.MovieId, x.Order });
    }
}