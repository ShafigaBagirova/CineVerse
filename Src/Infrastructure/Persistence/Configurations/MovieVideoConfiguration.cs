using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MovieVideoConfiguration : IEntityTypeConfiguration<MovieVideo>
{
    public void Configure(EntityTypeBuilder<MovieVideo> builder)
    {
        builder.ToTable("MovieVideos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VideoKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Site)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.IsOfficial)
            .IsRequired();

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.Videos)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}