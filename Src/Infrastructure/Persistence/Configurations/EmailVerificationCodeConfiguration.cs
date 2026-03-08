using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class EmailVerificationCodeConfiguration
    : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        builder.ToTable("EmailVerificationCodes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.Email, x.IsUsed });

        builder.HasIndex(x => x.Email);

        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
