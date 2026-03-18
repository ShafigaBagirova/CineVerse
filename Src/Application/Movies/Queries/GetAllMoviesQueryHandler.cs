using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Movies.Queries;


public sealed class GetAllMoviesQueryHandler
    : IRequestHandler<GetAllMoviesQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllMoviesQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllMoviesQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<GetAllMoviesQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetAllMoviesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetAllMoviesQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}",
            request.PageNumber,
            request.PageSize,
            request.SortBy);

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var cacheKey = $"movies:all:p{pageNumber}:s{pageSize}:sort:{request.SortBy}";

        var cachedResponse = await _cacheService.GetAsync<PaginatedResponse<GetAllMoviesResponse>>(cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("GetAllMoviesQuery cache hit for key {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("GetAllMoviesQuery cache miss for key {CacheKey}", cacheKey);

        var totalCount = await _movieRepository.CountAsync(cancellationToken);
        var movies = await _movieRepository.GetPagedAsync(pageNumber, pageSize, request.SortBy, cancellationToken);

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
            "GetAllMoviesQuery completed successfully. Returned {Count} items. TotalCount: {TotalCount}",
            items.Count,
            totalCount);

        return response;
    }
}