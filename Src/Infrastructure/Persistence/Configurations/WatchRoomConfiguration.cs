using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WatchRoomConfiguration : IEntityTypeConfiguration<WatchRoom>
{
    public void Configure(EntityTypeBuilder<WatchRoom> builder)
    {
        builder.ToTable("WatchRooms");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(16).IsRequired();
        builder.Property(x => x.HostUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Platform).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContentId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ContentUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.HostUserId);
    }
}
