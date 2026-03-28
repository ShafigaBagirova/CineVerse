using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.ToTable("FoodItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.ImageObjectKey)
            .HasMaxLength(500);

        builder.Property(x => x.IsAvailable)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.FoodCategory)
            .WithMany(x => x.FoodItems)
            .HasForeignKey(x => x.FoodCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Cinema)
            .WithMany(x => x.FoodItems)
            .OnDelete(DeleteBehavior.Restrict);
    }
}