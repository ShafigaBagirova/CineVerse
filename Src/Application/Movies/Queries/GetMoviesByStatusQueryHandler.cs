using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetMoviesByStatusQueryHandler
    : IRequestHandler<GetMoviesByStatusQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMoviesByStatusQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMoviesByStatusQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<GetMoviesByStatusQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetMoviesByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var cacheKey = $"movies:status:{request.Status}:p{pageNumber}:s{pageSize}";

        _logger.LogInformation(
            "GetMoviesByStatusQuery started. Status: {Status}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.Status,
            pageNumber,
            pageSize);

        var cachedResponse = await _cacheService
            .GetAsync<PaginatedResponse<GetAllMoviesResponse>>(cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("GetMoviesByStatusQuery cache hit for key {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("GetMoviesByStatusQuery cache miss for key {CacheKey}", cacheKey);

        var totalCount = await _movieRepository.CountByStatusAsync(request.Status, cancellationToken);

        var movies = await _movieRepository.GetByStatusPagedAsync(
            request.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = _mapper.Map<List<GetAllMoviesResponse>>(movies);

        var response = new PaginatedResponse<GetAllMoviesResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10), cancellationToken);

        _logger.LogInformation(
            "GetMoviesByStatusQuery completed successfully. Returned {Count} items. TotalCount: {TotalCount}",
            items.Count,
            totalCount);

        return response;
    }
}