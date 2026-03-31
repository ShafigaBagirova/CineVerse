using Application.AdminDashboard.Dtos;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AdminDashboard.Queries;

public sealed class GetAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetAdminDashboardSummaryQuery, AdminDashboardSummaryDto>
{
    private readonly IIdentityService _userRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ICinemaRepository _cinemaRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IMovieRatingRepository _ratingRepository;
    private readonly IWatchListItemRepository _watchlistRepository;
    private readonly ILogger<GetAdminDashboardSummaryQueryHandler> _logger;

    public GetAdminDashboardSummaryQueryHandler(
        IIdentityService userRepository,
        IMovieRepository movieRepository,
        ICinemaRepository cinemaRepository,
        IScreeningRepository screeningRepository,
        ITicketRepository ticketRepository,
        IPaymentRepository paymentRepository,
        IReviewRepository reviewRepository,
        IMovieRatingRepository ratingRepository,
        IWatchListItemRepository watchlistRepository,
        ILogger<GetAdminDashboardSummaryQueryHandler> logger)
    {
        _userRepository = userRepository;
        _movieRepository = movieRepository;
        _cinemaRepository = cinemaRepository;
        _screeningRepository = screeningRepository;
        _ticketRepository = ticketRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _ratingRepository = ratingRepository;
        _watchlistRepository = watchlistRepository;
        _logger = logger;
    }

    public async Task<AdminDashboardSummaryDto> Handle(
        GetAdminDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting admin dashboard summary.");

        var totalUsers = await _userRepository.CountUsersAsync(cancellationToken);
        var vipUsers = await _userRepository.CountVipUsersAsync(cancellationToken);

        var totalMovies = await _movieRepository.CountAsync(cancellationToken);
        var totalCinemas = await _cinemaRepository.CountAsync(cancellationToken);
        var activeScreenings = await _screeningRepository.CountActiveAsync(cancellationToken);

        var totalTicketsSold = await _ticketRepository.CountSoldTicketsAsync(cancellationToken);

        var successfulPayments = await _paymentRepository.CountByStatusAsync(
            PaymentStatus.Succeeded,
            cancellationToken);

        var failedPayments = await _paymentRepository.CountByStatusAsync(
            PaymentStatus.Failed,
            cancellationToken);

        var pendingPayments = await _paymentRepository.CountByStatusAsync(
            PaymentStatus.Pending,
            cancellationToken);

        var totalRevenue = await _paymentRepository.SumSuccessfulPaymentsAsync(cancellationToken);

        var totalReviews = await _reviewRepository.CountAsync(cancellationToken);
        var totalRatings = await _ratingRepository.CountAsync(cancellationToken);
        var totalWatchlistItems = await _watchlistRepository.CountAsync(cancellationToken);

        var response = new AdminDashboardSummaryDto
        {
            TotalUsers = totalUsers,
            VipUsers = vipUsers,
            TotalMovies = totalMovies,
            TotalCinemas = totalCinemas,
            ActiveScreenings = activeScreenings,
            TotalTicketsSold = totalTicketsSold,
            TotalRevenue = totalRevenue,
            SuccessfulPayments = successfulPayments,
            FailedPayments = failedPayments,
            PendingPayments = pendingPayments,
            TotalReviews = totalReviews,
            TotalRatings = totalRatings,
            TotalWatchlistItems = totalWatchlistItems
        };

        _logger.LogInformation(
            "Admin dashboard summary retrieved successfully. TotalUsers: {TotalUsers}, TotalRevenue: {TotalRevenue}",
            response.TotalUsers,
            response.TotalRevenue);

        return response;
    }
}