using Application.Common.Interfaces;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Application.Movies.Queries;

public sealed class GetMovieByIdQueryHandler
    : IRequestHandler<GetMovieByIdQuery, GetMovieByIdResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieRatingRepository _movieRatingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMovieProvider _movieProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMovieByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMovieByIdQueryHandler(
        IMovieRepository movieRepository,
        IMovieRatingRepository movieRatingRepository,
        ICurrentUserService currentUserService,
        IMovieProvider movieProvider,
        IMapper mapper,
        ILogger<GetMovieByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _movieRatingRepository = movieRatingRepository;
        _currentUserService = currentUserService;
        _movieProvider = movieProvider;
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

        var response = await _cacheService.GetAsync<GetMovieByIdResponse>(cacheKey, cancellationToken);

        if (response is null)
        {
            _logger.LogInformation("GetMovieByIdQuery cache miss for MovieId {MovieId}", request.Id);

            var movie = await _movieRepository.GetMovieWithDetailsByIdAsync(request.Id, cancellationToken);

            if (movie is null)
            {
                _logger.LogWarning("Movie with Id {MovieId} not found", request.Id);
                throw new KeyNotFoundException("Movie could not be found.");
            }

            response = _mapper.Map<GetMovieByIdResponse>(movie);
            response.Director = movie.Director;
            if (!string.IsNullOrWhiteSpace(movie.Actors))
            {
                response.Cast = movie.Actors
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            response.UserAverageRating = await _movieRatingRepository
                .GetAverageRatingAsync(request.Id, cancellationToken);

            response.RatingCount = await _movieRatingRepository
                .GetRatingsCountAsync(request.Id, cancellationToken);

            response.MyRating = null;

            if (movie.TmdbId is > 0)
            {
                var details = await _movieProvider.GetMovieDetailsAsync(movie.TmdbId.Value, cancellationToken);
                _logger.LogInformation(
                    "Movie detail TMDB enrichment for MovieId {MovieId}, TmdbId {TmdbId}. HasDetails={HasDetails}, Director={Director}, CastCount={CastCount}",
                    movie.Id,
                    movie.TmdbId.Value,
                    details is not null,
                    details?.Director,
                    details?.Cast?.Count ?? 0);
                if (details is not null)
                {
                    if (string.IsNullOrWhiteSpace(response.Director) && !string.IsNullOrWhiteSpace(details.Director))
                    {
                        response.Director = details.Director;
                    }

                    if (details.Cast.Count > 0)
                    {
                        response.Cast = details.Cast;
                    }
                }
            }
            else
            {
                _logger.LogInformation(
                    "Skipping TMDB enrichment for MovieId {MovieId} because TmdbId is empty.",
                    movie.Id);
            }

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10), cancellationToken);

            _logger.LogInformation("Movie with Id {MovieId} cached successfully", request.Id);
        }
        else
        {
            _logger.LogInformation("GetMovieByIdQuery cache hit for MovieId {MovieId}", request.Id);
        }

        var userId = _currentUserService.UserId;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var myRating = await _movieRatingRepository
                .GetByMovieAndUserAsync(request.Id, userId, cancellationToken);

            response.MyRating = myRating?.Rating;
        }


        return response;
    }
}