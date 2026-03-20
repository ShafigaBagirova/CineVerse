using Application.Common.Interfaces;
using Application.Follows.Dtos;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserSuggestionRepository:IUserSuggestionRepository
{
    private readonly CineVerseDbContext _context;

    public UserSuggestionRepository(CineVerseDbContext context)
    {
        _context = context;
    }
    public async Task<int> GetSuggestedUsersByTasteCountAsync(
    string currentUserId,
    CancellationToken cancellationToken)
    {
        var alreadyFollowingIds = _context.Follows
            .Where(x => x.FollowerId == currentUserId)
            .Select(x => x.FollowingId);

        var currentUserRatings = _context.MovieRatings
            .Where(x => x.UserId == currentUserId)
            .Select(x => new { x.MovieId, x.Rating });

        var query =
            from otherRating in _context.MovieRatings
            join currentRating in currentUserRatings
                on otherRating.MovieId equals currentRating.MovieId
            where otherRating.UserId != currentUserId
                  && !alreadyFollowingIds.Contains(otherRating.UserId)
            group new { otherRating, currentRating } by otherRating.UserId into g
            select new
            {
                UserId = g.Key,
                TasteScore = g.Sum(x =>
                    Math.Abs(x.otherRating.Rating - x.currentRating.Rating) == 0 ? 3 :
                    Math.Abs(x.otherRating.Rating - x.currentRating.Rating) == 1 ? 2 :
                    Math.Abs(x.otherRating.Rating - x.currentRating.Rating) == 2 ? 1 : 0),
                CommonMoviesCount = g.Count()
            };

        return await query
            .Where(x => x.TasteScore > 0)
            .CountAsync(cancellationToken);
    }
    public async Task<List<SuggestedUserScoreResult>> GetSuggestedUsersByTasteAsync(
    string currentUserId,
    int page,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var alreadyFollowingIds = _context.Follows
            .Where(x => x.FollowerId == currentUserId)
            .Select(x => x.FollowingId);

        var currentUserRatings = _context.MovieRatings
            .Where(x => x.UserId == currentUserId)
            .Select(x => new { x.MovieId, x.Rating });

        var query =
            from otherRating in _context.MovieRatings
            join currentRating in currentUserRatings
                on otherRating.MovieId equals currentRating.MovieId
            where otherRating.UserId != currentUserId
                  && !alreadyFollowingIds.Contains(otherRating.UserId)
            group new { otherRating, currentRating } by otherRating.UserId into g
            select new SuggestedUserScoreResult
            {
                UserId = g.Key,
                TasteScore = g.Sum(x =>
                    Math.Abs(x.otherRating.Rating - x.currentRating.Rating) == 0 ? 3 :
                    Math.Abs(x.otherRating.Rating - x.currentRating.Rating) == 1 ? 2 :
                    Math.Abs(x.otherRating.Rating - x.currentRating.Rating) == 2 ? 1 : 0),
                CommonMoviesCount = g.Count()
            };

        return await query
            .Where(x => x.TasteScore > 0)
            .OrderByDescending(x => x.TasteScore)
            .ThenByDescending(x => x.CommonMoviesCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
