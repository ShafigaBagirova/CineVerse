using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using Application.Movies.Dtos;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Infrastructure.Recommendation;

public sealed class RecommendationService : IRecommendationService
{
    private readonly CineVerseDbContext _context;
    private readonly IUserSuggestionRepository _userSuggestionRepository;
    private readonly IIdentityService _identityService;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        CineVerseDbContext context,
        IUserSuggestionRepository userSuggestionRepository,
        IIdentityService identityService,
        ILogger<RecommendationService> logger)
    {
        _context = context;
        _userSuggestionRepository = userSuggestionRepository;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetSuggestedMoviesResponse>> GetSuggestedMoviesAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var likedMovieIds = await _context.MovieRatings
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Rating >= 4)
            .Select(x => x.MovieId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (likedMovieIds.Count == 0)
        {
            return new PaginatedResponse<GetSuggestedMoviesResponse>
            {
                Items = [],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        var preferredGenreIds = await _context.MovieGenres
            .AsNoTracking()
            .Where(x => likedMovieIds.Contains(x.MovieId))
            .Select(x => x.GenreId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var excludedMovieIds = await _context.MovieRatings
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.MovieId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = _context.Movies
            .AsNoTracking()
            .Where(x => x.Status==MovieStatus.Released)
            .Where(x => !excludedMovieIds.Contains(x.Id))
            .Where(x => x.MovieGenres.Any(mg => preferredGenreIds.Contains(mg.GenreId)));

        var totalCount = await query.CountAsync(cancellationToken);

        var movies = await query
            .OrderByDescending(x => x.ImdbRating)
            .ThenByDescending(x => x.ReleaseDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GetSuggestedMoviesResponse
            {
                Id = x.Id,
                Title = x.Title,
                PosterUrl = x.PosterPath,
                ImdbRating = x.ImdbRating?? 0,
                ReleaseDate = x.ReleaseDate.HasValue?x.ReleaseDate.Value.Year:0,
                Slug = x.Slug,
                Genres = x.MovieGenres
                    .Select(mg => mg.Genre.Name)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<GetSuggestedMoviesResponse>
        {
            Items = movies,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    public async Task<PaginatedResponse<SuggestedUserItemDto>> GetSuggestedUsersAsync(
       string userId,
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken)
    {
        var tasteTotal = await _userSuggestionRepository.GetSuggestedUsersByTasteCountAsync(
            userId,
            cancellationToken);

     
        if (tasteTotal > 0)
        {
            var suggestedScores = await _userSuggestionRepository.GetSuggestedUsersByTasteAsync(
                userId,
                pageNumber,
                pageSize,
                cancellationToken);

            var scoreUserIds = suggestedScores.Select(x => x.UserId).ToList();
            var usersById = await _identityService.GetUsersByIdsAsync(scoreUserIds, cancellationToken);

            var tasteItems = suggestedScores
                .Select(score =>
                {
                    var user = usersById.FirstOrDefault(x => x.UserId == score.UserId);
                    if (user is null) return null;

                    return new SuggestedUserItemDto
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        AvatarUrl = user.AvatarUrl,
                        TasteScore = score.TasteScore,
                        CommonMoviesCount = score.CommonMoviesCount
                    };
                })
                .Where(x => x is not null)
                .Cast<SuggestedUserItemDto>()
                .ToList();

            if (tasteItems.Count > 0 || pageNumber > 1)
            {
                return new PaginatedResponse<SuggestedUserItemDto>
                {
                    Items = tasteItems,
                    TotalCount = tasteTotal,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        return await GetDirectorySuggestedUsersAsync(
            userId,
            pageNumber,
            pageSize,
            cancellationToken);
    }

    private async Task<PaginatedResponse<SuggestedUserItemDto>> GetDirectorySuggestedUsersAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var totalUsersCount = await _context.Users.AsNoTracking().CountAsync(cancellationToken);

        var usersExcludingCurrentQuery = _context.Users.AsNoTracking().Where(u => u.Id != userId);
        var usersExcludingCurrentCount = await usersExcludingCurrentQuery.CountAsync(cancellationToken);

        var vipUsersCount = await _context.Users
            .AsNoTracking()
            .CountAsync(
                u => u.Id != userId
                    && (u.IsVip || (u.VipExpiresAt.HasValue && u.VipExpiresAt.Value > now)),
                cancellationToken);

        var finalFallbackCount = usersExcludingCurrentCount;

        _logger.LogInformation(
            "Suggested users directory metrics. CurrentUserId: {CurrentUserId}, TotalUsers: {TotalUsers}, AfterExcludingCurrent: {AfterExcludingCurrent}, VipUsers: {VipUsers}, FinalFallbackTotal: {FinalFallbackTotal}",
            userId,
            totalUsersCount,
            usersExcludingCurrentCount,
            vipUsersCount,
            finalFallbackCount);

        var orderedIds = await usersExcludingCurrentQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var items = await MapUserIdsToSuggestedItemsAsync(orderedIds, cancellationToken);

        return new PaginatedResponse<SuggestedUserItemDto>
        {
            Items = items,
            TotalCount = finalFallbackCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private async Task<List<SuggestedUserItemDto>> MapUserIdsToSuggestedItemsAsync(
        List<string> orderedIds,
        CancellationToken cancellationToken)
    {
        if (orderedIds.Count == 0)
            return [];

        var profileRows = await _identityService.GetUsersByIdsAsync(orderedIds, cancellationToken);
        var byId = profileRows.ToDictionary(x => x.UserId, StringComparer.Ordinal);

        var items = new List<SuggestedUserItemDto>();
        foreach (var id in orderedIds)
        {
            if (!byId.TryGetValue(id, out var u))
                continue;

            items.Add(new SuggestedUserItemDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                TasteScore = 0,
                CommonMoviesCount = 0
            });
        }

        return items;
    }
}