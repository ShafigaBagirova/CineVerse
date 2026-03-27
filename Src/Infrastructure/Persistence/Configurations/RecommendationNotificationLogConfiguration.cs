using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RecommendationNotificationLogConfiguration
    : IEntityTypeConfiguration<RecommendationNotificationLog>
{
    public void Configure(EntityTypeBuilder<RecommendationNotificationLog> builder)
    {
        builder.ToTable("RecommendationNotificationLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.TargetType)
            .IsRequired();

        builder.Property(x => x.TargetKey)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.TargetType, x.TargetKey })
            .IsUnique();
    }
}