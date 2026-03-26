using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProcessedWebhookEventConfiguration : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.ToTable(nameof(ProcessedWebhookEvent));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.EventId)
            .IsUnique();

        builder.Property(x => x.ProcessedAtUtc)
            .IsRequired();
    }
}
