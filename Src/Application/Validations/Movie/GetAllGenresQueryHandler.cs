using Application.Common.Interfaces;
using Application.Movies.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Validations.Movie;

public sealed class GetAllGenresQueryHandler
    : IRequestHandler<GetAllGenresQuery, List<GetAllGenresResponse>>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetAllGenresQueryHandler> _logger;

    public GetAllGenresQueryHandler(
        IGenreRepository genreRepository,
        ICacheService cacheService,
        ILogger<GetAllGenresQueryHandler> logger)
    {
        _genreRepository = genreRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<GetAllGenresResponse>> Handle(
        GetAllGenresQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = "genres:all";

        _logger.LogInformation("GetAllGenres started");

        var cached = await _cacheService
            .GetAsync<List<GetAllGenresResponse>>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            _logger.LogInformation("GetAllGenres cache hit");
            return cached;
        }

        _logger.LogInformation("GetAllGenres cache miss");

        var genres = await _genreRepository
            .GetAllAsync(cancellationToken);

        var response = genres
            .Select(x => new GetAllGenresResponse
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        await _cacheService.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromHours(1),
            cancellationToken);

        _logger.LogInformation("GetAllGenres cached successfully");

        return response;
    }
}