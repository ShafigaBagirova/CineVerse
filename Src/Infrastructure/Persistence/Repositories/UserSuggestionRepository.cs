using Application.Common.Interfaces;
using Application.Follows.Dtos;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserSuggestionRepository : IUserSuggestionRepository
{
    private readonly CineVerseDbContext _context;

   
    private const int WatchOverlapPointsPerMovie = 2;

    public UserSuggestionRepository(CineVerseDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetSuggestedUsersByTasteCountAsync(
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var merged = await GetMergedScoresAsync(currentUserId, cancellationToken);
        return merged.Count;
    }

    public async Task<List<SuggestedUserScoreResult>> GetSuggestedUsersByTasteAsync(
        string currentUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var merged = await GetMergedScoresAsync(currentUserId, cancellationToken);
        return merged
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    private async Task<List<SuggestedUserScoreResult>> GetMergedScoresAsync(
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var alreadyFollowingIds = _context.Follows
            .Where(x => x.FollowerId == currentUserId)
            .Select(x => x.FollowingId);

        var currentUserRatings = _context.MovieRatings
            .Where(x => x.UserId == currentUserId)
            .Select(x => new { x.MovieId, x.Rating });

        var ratingQuery =
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

        var ratingScores = await ratingQuery.ToListAsync(cancellationToken);

        var bothRatedPairs = await (
            from r1 in _context.MovieRatings
            where r1.UserId == currentUserId
            join r2 in _context.MovieRatings on r1.MovieId equals r2.MovieId
            where r2.UserId != currentUserId && !alreadyFollowingIds.Contains(r2.UserId)
            select new { OtherId = r2.UserId, r1.MovieId }
        ).ToListAsync(cancellationToken);

        var bothRatedSet = bothRatedPairs.Select(x => (x.OtherId, x.MovieId)).ToHashSet();

        var watchPairs = await (
            from ow in _context.WatchLogs
            join cw in _context.WatchLogs.Where(w => w.UserId == currentUserId) on ow.MovieId equals cw.MovieId
            where ow.UserId != currentUserId && !alreadyFollowingIds.Contains(ow.UserId)
            select new { ow.UserId, ow.MovieId }
        ).ToListAsync(cancellationToken);

        var watchBonusByUser = watchPairs
            .Where(p => !bothRatedSet.Contains((p.UserId, p.MovieId)))
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.Count());

        var merged = new Dictionary<string, SuggestedUserScoreResult>();

        foreach (var r in ratingScores)
        {
            merged[r.UserId] = new SuggestedUserScoreResult
            {
                UserId = r.UserId,
                TasteScore = r.TasteScore,
                CommonMoviesCount = r.CommonMoviesCount
            };
        }

        foreach (var kv in watchBonusByUser)
        {
            if (!merged.TryGetValue(kv.Key, out var row))
            {
                row = new SuggestedUserScoreResult { UserId = kv.Key, TasteScore = 0, CommonMoviesCount = 0 };
                merged[kv.Key] = row;
            }

            row.TasteScore += kv.Value * WatchOverlapPointsPerMovie;
            row.CommonMoviesCount += kv.Value;
        }

        return merged.Values
            .Where(x => x.TasteScore > 0)
            .OrderByDescending(x => x.TasteScore)
            .ThenByDescending(x => x.CommonMoviesCount)
            .ToList();
    }
}
