using Application.AdminDashboard.Dtos;
using Application.Common.Interfaces;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Infrastructure.Persistence.Repositories;

public sealed class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly CineVerseDbContext _context;
    private readonly UserManager<CineVerseUser> _userManager;
    public AdminDashboardRepository(CineVerseDbContext context,UserManager<CineVerseUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<RevenueChartItemDto>> GetRevenueChartAsync(int days, CancellationToken cancellationToken)
    {
        var fromDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        var payments = await _context.Payments
            .Where(x => x.Status == PaymentStatus.Succeeded && x.PaidAtUtc >= fromDate)
            .ToListAsync(cancellationToken);

        var tickets = await _context.Tickets
            .Where(x => x.PurchasedAtUtc >= fromDate)
            .ToListAsync(cancellationToken);

        var dates = Enumerable.Range(0, days)
            .Select(i => fromDate.AddDays(i))
            .ToList();

        return dates.Select(date => new RevenueChartItemDto
        {
            Date = date,
            Revenue = payments
                .Where(x => x.PaidAtUtc.HasValue && x.PaidAtUtc.Value.Date == date.Date)
                .Sum(x => x.TotalAmount),
            TicketsSold = tickets
                .Count(x => x.PurchasedAtUtc.Date == date.Date)
        }).ToList();
    }

    public async Task<List<TopMovieDto>> GetTopMoviesAsync(int take, CancellationToken cancellationToken)
    {
        return await _context.Movies
            .Select(m => new TopMovieDto
            {
                MovieId = m.Id,
                Title = m.Title,
                AverageRating = m.MovieRatings.Any() ? m.MovieRatings.Average(r => r.Rating) : 0,
                ReviewCount = m.Reviews.Count,
                WatchlistCount = m.WatchlistItems.Count,
                WatchCount = m.WatchLogs.Count,
                TicketCount = m.Screenings.SelectMany(s => s.Tickets).Count(),
                Revenue = m.Screenings.SelectMany(s => s.Tickets).Sum(t => (decimal?)t.Price) ?? 0m
            })
            .OrderByDescending(x => x.TicketCount)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RecentPaymentDto>> GetRecentPaymentsAsync(int take, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .OrderByDescending(x => x.PaidAtUtc)
            .Take(take)
            .Select(x => new RecentPaymentDto
            {
                PaymentId = x.Id,
                UserId = x.UserId,
                Amount = x.TotalAmount,
                Status = x.Status,
                CreatedAt = x.PaidAtUtc

            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RecentUserDto>> GetRecentUsersAsync(int take, CancellationToken cancellationToken)
    {
        return await _userManager.Users
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new RecentUserDto
            {
                UserId = x.Id,
                UserName = x.UserName ?? string.Empty,
                Email = x.Email ?? string.Empty,
                IsVip = x.IsVip,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ScreeningOccupancyDto>> GetScreeningOccupancyAsync(int take, CancellationToken cancellationToken)
    {
        return await _context.Screenings
            .Select(s => new ScreeningOccupancyDto
            {
                ScreeningId = s.Id,
                MovieTitle = s.Movie.Title,
                CinemaName = s.Hall.Cinema.Name,
                HallName = s.Hall.Name,
                StartTime = s.StartTime,
                TotalSeats = s.Hall.Seats.Count,
                SoldSeats = s.Tickets.Count,
                OccupancyRate = s.Hall.Seats.Count == 0
                    ? 0
                    : (double)s.Tickets.Count * 100 / s.Hall.Seats.Count
            })
            .OrderByDescending(x => x.OccupancyRate)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}