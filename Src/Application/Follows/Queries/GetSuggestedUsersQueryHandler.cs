using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetSuggestedUsersQueryHandler
    : IRequestHandler<GetSuggestedUsersQuery, BaseResponse<PaginatedResponse<SuggestedUserItemDto>>>
{
    private readonly IUserSuggestionRepository _userSuggestionRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetSuggestedUsersQueryHandler> _logger;

    public GetSuggestedUsersQueryHandler(
        IUserSuggestionRepository userSuggestionRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        ILogger<GetSuggestedUsersQueryHandler> logger)
    {
        _userSuggestionRepository = userSuggestionRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<SuggestedUserItemDto>>> Handle(
        GetSuggestedUsersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetSuggestedUsersQuery started. Page: {Page}, PageSize: {PageSize}",
            request.Page,
            request.PageSize);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<PaginatedResponse<SuggestedUserItemDto>>.Fail("User is not authenticated.");

        var suggestedScores = await _userSuggestionRepository.GetSuggestedUsersByTasteAsync(
            currentUserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _userSuggestionRepository.GetSuggestedUsersByTasteCountAsync(
            currentUserId,
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

        var paginated = new PaginatedResponse<SuggestedUserItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };

        _logger.LogInformation(
            "GetSuggestedUsersQuery completed successfully. CurrentUserId: {CurrentUserId}, ReturnedCount: {ReturnedCount}",
            currentUserId,
            items.Count);

        return BaseResponse<PaginatedResponse<SuggestedUserItemDto>>.Ok(
            paginated,
            "Suggested users retrieved successfully.");
    }
}