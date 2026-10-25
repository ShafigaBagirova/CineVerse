using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WatchRoomMemberConfiguration : IEntityTypeConfiguration<WatchRoomMember>
{
    public void Configure(EntityTypeBuilder<WatchRoomMember> builder)
    {
        builder.ToTable("WatchRoomMembers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();

        builder.HasOne(x => x.WatchRoom)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.WatchRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.WatchRoomId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
