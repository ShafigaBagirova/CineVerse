using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetMoviesByLanguageQueryHandler
    : IRequestHandler<GetMoviesByLanguageQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMoviesByLanguageQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMoviesByLanguageQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<GetMoviesByLanguageQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetMoviesByLanguageQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language.Trim().ToLower();
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var cacheKey = $"movies:language:{language}:p{pageNumber}:s{pageSize}";

        _logger.LogInformation(
            "GetMoviesByLanguageQuery started. Language: {Language}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            language, pageNumber, pageSize);

        var cachedResponse = await _cacheService
            .GetAsync<PaginatedResponse<GetAllMoviesResponse>>(cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

        var totalCount = await _movieRepository.CountByLanguageAsync(language, cancellationToken);

        var movies = await _movieRepository.GetByLanguagePagedAsync(
            language,
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
            "GetMoviesByLanguageQuery completed. Returned {Count} items. TotalCount: {TotalCount}",
            items.Count, totalCount);

        return response;
    }
}