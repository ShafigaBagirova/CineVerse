using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("Follows");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FollowerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.FollowingId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.FollowerId, x.FollowingId })
            .IsUnique();

        builder.ToTable("Follows", t =>
        {
            t.HasCheckConstraint(
                "CK_Follows_NoSelfFollow",
                "[FollowerId] <> [FollowingId]");
        });

    }
}