using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

public class CineVerseDbContext : IdentityDbContext<CineVerseUser>
{
    public CineVerseDbContext(DbContextOptions<CineVerseDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CineVerseDbContext).Assembly);
    }
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MoviePoster> MoviePosters { get; set; }
}
