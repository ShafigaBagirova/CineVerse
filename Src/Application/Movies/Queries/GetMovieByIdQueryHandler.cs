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
    private readonly IMovieRatingRepository _movieRatingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMovieByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMovieByIdQueryHandler(
        IMovieRepository movieRepository,
        IMovieRatingRepository movieRatingRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetMovieByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _movieRatingRepository = movieRatingRepository;
        _currentUserService = currentUserService;
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

            response.UserAverageRating = await _movieRatingRepository
                .GetAverageRatingAsync(request.Id, cancellationToken);

            response.RatingCount = await _movieRatingRepository
                .GetRatingsCountAsync(request.Id, cancellationToken);

            response.MyRating = null;

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