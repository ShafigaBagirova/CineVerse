using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Application.Movies.Queries;

public class GetAllMoviesQueryHandler
    : IRequestHandler<GetAllMoviesQuery, BaseResponse<PaginatedResponse<GetAllMoviesResponse>>>
{
    private readonly IMovieRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllMoviesQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllMoviesQueryHandler(
        IMovieRepository repository,
        IMapper mapper,
        ILogger<GetAllMoviesQueryHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllMoviesResponse>>> Handle(
        GetAllMoviesQuery request,
        CancellationToken cancellationToken)
    {
        var actorForFilter = new[] { request.Request.Actor, request.Request.ActorName, request.Request.Cast }
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        var directorForFilter = new[] { request.Request.Director, request.Request.DirectorName }
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

        _logger.LogInformation(
            "GetAllMoviesQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}, Search: {Search}, Actor: {Actor}, Director: {Director}, GenreId: {GenreId}, Language: {Language}, Status: {Status}, Year: {Year}, MinTmdbRating: {MinTmdbRating}, MaxTmdbRating: {MaxTmdbRating}, MinUserRating: {MinUserRating}, MaxUserRating: {MaxUserRating}, SortBy: {SortBy}, Desc: {Desc}",
            request.Request.PageNumber,
            request.Request.PageSize,
            request.Request.Search,
            actorForFilter,
            directorForFilter,
            request.Request.GenreId,
            request.Request.Language,
            request.Request.Status,
            request.Request.Year,
            request.Request.MinTmdbRating,
            request.Request.MaxTmdbRating,
            request.Request.MinUserRating,
            request.Request.MaxUserRating,
            request.Request.SortBy,
            request.Request.Desc);

        var cacheKey = CacheKeys.MoviesPaged(
          request.Request.PageNumber,
          request.Request.PageSize,
          request.Request.Search,
          actorForFilter,
          directorForFilter,
          request.Request.GenreId,
          request.Request.Language,
          request.Request.Status,
          request.Request.Year,
          request.Request.MinTmdbRating,
          request.Request.MaxTmdbRating,
          request.Request.MinUserRating,
          request.Request.MaxUserRating,
          request.Request.SortBy,
          request.Request.Desc
      );

        var cachedResponse =
            await _cacheService.GetAsync<PaginatedResponse<GetAllMoviesResponse>>(cacheKey);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("GetAllMoviesQuery response fetched from cache.");

            return BaseResponse<PaginatedResponse<GetAllMoviesResponse>>
                .Ok(cachedResponse, "Movies fetched from cache");
        }

        var result = await _repository.GetPagedAsync(
            request.Request.PageNumber,
            request.Request.PageSize,
            request.Request.Search,
            actorForFilter,
            directorForFilter,
            request.Request.GenreId,
            request.Request.Language,
            request.Request.Status,
            request.Request.Year,
            request.Request.MinTmdbRating,
            request.Request.MaxTmdbRating,
            request.Request.MinUserRating,
            request.Request.MaxUserRating,
            request.Request.SortBy,
            request.Request.Desc,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetAllMoviesResponse>>(result.Items);

        var totalPages = (int)Math.Ceiling((double)result.TotalCount / request.Request.PageSize);

        var paginatedResponse = new PaginatedResponse<GetAllMoviesResponse>
        {
            Items = mappedItems,
            TotalCount = result.TotalCount,
            PageNumber = request.Request.PageNumber,
            PageSize = request.Request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.Request.PageNumber > 1,
            HasNextPage = request.Request.PageNumber < totalPages
        };

        await _cacheService.SetAsync(cacheKey, paginatedResponse, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetAllMoviesQuery completed successfully. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            mappedItems.Count,
            result.TotalCount);

        return BaseResponse<PaginatedResponse<GetAllMoviesResponse>>
            .Ok(paginatedResponse, "Movies fetched successfully");
    }
}