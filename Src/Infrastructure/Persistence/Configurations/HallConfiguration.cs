using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class HallConfiguration: IEntityTypeConfiguration<Hall>
{

    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Halls"); 
       
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(h => h.Capacity)
            .IsRequired();
       
        builder.Property(h=>h.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(builder => builder.Cinema)
            .WithMany(cinema => cinema.Halls)
            .HasForeignKey(builder => builder.CinemaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
