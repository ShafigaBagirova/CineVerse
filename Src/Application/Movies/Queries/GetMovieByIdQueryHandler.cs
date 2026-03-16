using Application.Common.Interfaces;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetMovieByIdQueryHandler
    : IRequestHandler<GetMovieByIdQuery, GetMovieByIdResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMovieByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMovieByIdQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<GetMovieByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<GetMovieByIdResponse> Handle(
        GetMovieByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"movies:id:{request.Id}";

        _logger.LogInformation("GetMovieByIdQuery started for MovieId {MovieId}", request.Id);

        var cachedMovie = await _cacheService.GetAsync<GetMovieByIdResponse>(cacheKey, cancellationToken);

        if (cachedMovie is not null)
        {
            _logger.LogInformation("GetMovieByIdQuery cache hit for MovieId {MovieId}", request.Id);
            return cachedMovie;
        }

        _logger.LogInformation("GetMovieByIdQuery cache miss for MovieId {MovieId}", request.Id);

        var movie = await _movieRepository.GetByIdWithMediaAsync(request.Id, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning("Movie with Id {MovieId} not found", request.Id);
            throw new KeyNotFoundException("Movie could not be found.");
        }

        var response = _mapper.Map<GetMovieByIdResponse>(movie);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10), cancellationToken);

        _logger.LogInformation("Movie with Id {MovieId} cached successfully", request.Id);

        return response;
    }
}