using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Watched.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Watched.Queries;

public sealed class GetMyWatchedMoviesQueryHandler
    : IRequestHandler<GetMyWatchedMoviesQuery, BaseResponse<PaginatedResponse<WatchedMovieDto>>>
{
    private readonly IWatchLogRepository _watchLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyWatchedMoviesQueryHandler> _logger;

    public GetMyWatchedMoviesQueryHandler(
        IWatchLogRepository watchLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetMyWatchedMoviesQueryHandler> logger)
    {
        _watchLogRepository = watchLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<WatchedMovieDto>>> Handle(
        GetMyWatchedMoviesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetMyWatchedMoviesQuery started. Page: {Page}, PageSize: {PageSize}",
            request.Page,
            request.PageSize);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetMyWatchedMoviesQuery failed. Authenticated user not found.");

            return BaseResponse<PaginatedResponse<WatchedMovieDto>>
                .Fail("Authenticated user could not be found.");
        }

        var totalCount = await _watchLogRepository.GetCountByUserIdAsync(
            userId,
            cancellationToken);

        var watchedMovies = await _watchLogRepository.GetPagedByUserIdAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var watchedDtos = _mapper.Map<List<WatchedMovieDto>>(watchedMovies);

        var result = new PaginatedResponse<WatchedMovieDto>
        {
            Items = watchedDtos,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "GetMyWatchedMoviesQuery completed successfully. UserId: {UserId}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            userId,
            watchedDtos.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<WatchedMovieDto>>.Ok(result);
    }
}
