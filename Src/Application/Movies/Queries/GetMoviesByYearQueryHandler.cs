using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetMoviesByYearQueryHandler
    : IRequestHandler<GetMoviesByYearQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMoviesByYearQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMoviesByYearQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<GetMoviesByYearQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetMoviesByYearQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var cacheKey = $"movies:year:{request.Year}:p{pageNumber}:s{pageSize}";

        _logger.LogInformation(
            "GetMoviesByYearQuery started. Year: {Year}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.Year,
            pageNumber,
            pageSize);

        var cachedResponse = await _cacheService
            .GetAsync<PaginatedResponse<GetAllMoviesResponse>>(cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

        var totalCount = await _movieRepository.CountByYearAsync(request.Year, cancellationToken);

        var movies = await _movieRepository.GetByYearPagedAsync(
            request.Year,
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
            "GetMoviesByYearQuery completed successfully. Returned {Count} items. TotalCount: {TotalCount}",
            items.Count,
            totalCount);

        return response;
    }
}