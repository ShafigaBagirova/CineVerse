using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class FoodCategoryConfiguration : IEntityTypeConfiguration<FoodCategory>
{
    public void Configure(EntityTypeBuilder<FoodCategory> builder)
    {
        builder.ToTable("FoodCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasMany(x => x.FoodItems)
            .WithOne(x => x.FoodCategory)
            .HasForeignKey(x => x.FoodCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.Cinema)
            .WithMany(x=>x.FoodCategories)
            .OnDelete(DeleteBehavior.Restrict);
    }
}