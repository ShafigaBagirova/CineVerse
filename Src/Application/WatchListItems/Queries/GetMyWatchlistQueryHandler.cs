using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.WatchListItems.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchListItems.Queries;

public sealed class GetMyWatchlistQueryHandler
    : IRequestHandler<GetMyWatchlistQuery, BaseResponse<PaginatedResponse<WatchlistMovieDto>>>
{
    private readonly IWatchListItemRepository _watchlistRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyWatchlistQueryHandler> _logger;

    public GetMyWatchlistQueryHandler(
        IWatchListItemRepository watchlistRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetMyWatchlistQueryHandler> logger)
    {
        _watchlistRepository = watchlistRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<WatchlistMovieDto>>> Handle(
        GetMyWatchlistQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetMyWatchlistQuery started. Page: {Page}, PageSize: {PageSize}",
            request.Page,
            request.PageSize);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetMyWatchlistQuery failed. Authenticated user not found.");

            return BaseResponse<PaginatedResponse<WatchlistMovieDto>>
                .Fail("Authenticated user could not be found.");
        }

        var totalCount = await _watchlistRepository.GetCountByUserIdAsync(
            userId,
            cancellationToken);

        var watchlistItems = await _watchlistRepository.GetPagedByUserIdAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var watchlistDtos = _mapper.Map<List<WatchlistMovieDto>>(watchlistItems);

        var result = new PaginatedResponse<WatchlistMovieDto>
        {
            Items = watchlistDtos,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "GetMyWatchlistQuery completed successfully. UserId: {UserId}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            userId,
            watchlistDtos.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<WatchlistMovieDto>>.Ok(result);
    }
}