using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class WatchLogConfiguration : IEntityTypeConfiguration<WatchLog>
{
    public void Configure(EntityTypeBuilder<WatchLog> builder)
    {
        builder.ToTable("WatchLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.MovieId })
            .IsUnique();

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.WatchLogs)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}