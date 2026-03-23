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
    public DbSet<MovieVideo> MovieVideos { get; set; }
    public DbSet<MovieRating> MovieRatings { get; set; }
    public DbSet<Genre> Genres {  get; set; }
    public DbSet<MovieGenre> MovieGenres {  get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<WatchListItem> WatchlistItems {  get; set; }
    public DbSet<WatchLog> WatchLogs { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Cinema> Cinemas { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Screening> Screenings { get; set; }
}
