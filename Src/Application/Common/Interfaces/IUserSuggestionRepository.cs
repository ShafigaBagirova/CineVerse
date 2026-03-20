using Application.Follows.Dtos;

namespace Application.Common.Interfaces;

public interface IUserSuggestionRepository
{
    Task<List<SuggestedUserScoreResult>> GetSuggestedUsersByTasteAsync(
        string currentUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> GetSuggestedUsersByTasteCountAsync(
        string currentUserId,
        CancellationToken cancellationToken);
}