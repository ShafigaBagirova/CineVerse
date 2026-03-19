using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetMoviesByGenreQueryHandler
    : IRequestHandler<GetMoviesByGenreQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<GetMoviesByGenreQueryHandler> _logger;
    private readonly IGenreRepository _genreRepository;
    private readonly ICacheService _cacheService;

    public GetMoviesByGenreQueryHandler(
        IMovieRepository movieRepository,
        ILogger<GetMoviesByGenreQueryHandler> logger,
        IGenreRepository genreRepository,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _logger = logger;
        _genreRepository = genreRepository;
        _cacheService = cacheService;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetMoviesByGenreQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetMoviesByGenre started. GenreId: {GenreId}, Page: {Page}, PageSize: {PageSize}",
            request.GenreId,
            request.Page,
            request.PageSize);
        var cacheKey = $"movies:genre:{request.GenreId}:page:{request.Page}:size:{request.PageSize}";

        var cachedResponse = await _cacheService
            .GetAsync<PaginatedResponse<GetAllMoviesResponse>>(cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation(
                "GetMoviesByGenre cache hit. GenreId: {GenreId}, Page: {Page}, PageSize: {PageSize}",
                request.GenreId,
                request.Page,
                request.PageSize);

            return cachedResponse;
        }

        _logger.LogInformation(
            "GetMoviesByGenre cache miss. GenreId: {GenreId}, Page: {Page}, PageSize: {PageSize}",
            request.GenreId,
            request.Page,
            request.PageSize);

        var genreExists = await _genreRepository
    .ExistsAsync(request.GenreId, cancellationToken);

        if (!genreExists)
        {
            _logger.LogWarning("Genre not found. GenreId: {GenreId}", request.GenreId);
            throw new KeyNotFoundException("Genre not found.");
        }

        var totalCount = await _movieRepository
            .GetMoviesCountByGenreAsync(request.GenreId, cancellationToken);

        var movies = await _movieRepository
            .GetMoviesByGenreAsync(request.GenreId, request.Page, request.PageSize, cancellationToken);



        var response = movies.Select(m => new GetAllMoviesResponse
        {
            Id = m.Id,
            Title = m.Title,
            UserAverageRating = m.UserAverageRating
        }).ToList();

        var result = new PaginatedResponse<GetAllMoviesResponse>
        {
            Items = response,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(10),
            cancellationToken);

        _logger.LogInformation(
            "GetMoviesByGenre completed and cached. GenreId: {GenreId}, Count: {Count}",
            request.GenreId,
            response.Count);

        return result;
    }
}