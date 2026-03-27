using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using Application.Movies.Dtos;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Recommendation;

public sealed class RecommendationService : IRecommendationService
{
    private readonly CineVerseDbContext _context;
    private readonly IUserSuggestionRepository _userSuggestionRepository;
    private readonly IIdentityService _identityService;

    public RecommendationService(CineVerseDbContext context, IUserSuggestionRepository userSuggestionRepository, IIdentityService identityService)
    {
        _context = context;
        _userSuggestionRepository = userSuggestionRepository;
        _identityService = identityService;
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
        var suggestedScores = await _userSuggestionRepository.GetSuggestedUsersByTasteAsync(
            userId,
            pageNumber,
            pageSize,
            cancellationToken);

        var totalCount = await _userSuggestionRepository.GetSuggestedUsersByTasteCountAsync(
            userId,
            cancellationToken);

        var userIds = suggestedScores.Select(x => x.UserId).ToList();

        var users = await _identityService.GetUsersByIdsAsync(userIds, cancellationToken);

        var items = suggestedScores
            .Select(score =>
            {
                var user = users.FirstOrDefault(x => x.UserId == score.UserId);
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

        return new PaginatedResponse<SuggestedUserItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}