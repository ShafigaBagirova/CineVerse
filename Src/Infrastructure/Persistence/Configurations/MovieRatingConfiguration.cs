using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MovieRatingConfiguration : IEntityTypeConfiguration<MovieRating>
{
    public void Configure(EntityTypeBuilder<MovieRating> builder)
    {
        builder.ToTable("MovieRatings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
        .IsRequired()
        .HasPrecision(3, 1);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.MovieRatings)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasIndex(x => new { x.MovieId, x.UserId })
            .IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_MovieRatings_Rating", "[Rating] >= 1 AND [Rating] <= 10");
        });
    }
}