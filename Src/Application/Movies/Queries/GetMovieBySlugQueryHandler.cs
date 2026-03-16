using Application.Common.Interfaces;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;


public sealed class GetMovieBySlugQueryHandler
    : IRequestHandler<GetMovieBySlugQuery, GetMovieByIdResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMovieBySlugQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMovieBySlugQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<GetMovieBySlugQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<GetMovieByIdResponse> Handle(
        GetMovieBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLower();
        var cacheKey = $"movies:slug:{slug}";

        _logger.LogInformation("GetMovieBySlugQuery started for Slug {Slug}", slug);

        var cachedMovie = await _cacheService.GetAsync<GetMovieByIdResponse>(cacheKey, cancellationToken);

        if (cachedMovie is not null)
        {
            _logger.LogInformation("GetMovieBySlugQuery cache hit for Slug {Slug}", slug);
            return cachedMovie;
        }

        _logger.LogInformation("GetMovieBySlugQuery cache miss for Slug {Slug}", slug);

        var movie = await _movieRepository.GetBySlugWithMediaAsync(slug, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning("Movie with slug {Slug} not found", slug);
            throw new KeyNotFoundException("Movie tapılmadı.");
        }

        var response = _mapper.Map<GetMovieByIdResponse>(movie);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10), cancellationToken);

        _logger.LogInformation("Movie with slug {Slug} cached successfully", slug);

        return response;
    }
}