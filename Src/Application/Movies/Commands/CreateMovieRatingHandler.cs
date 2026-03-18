using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public class CreateMovieRatingCommandHandler
    : IRequestHandler<CreateMovieRatingCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieRatingRepository _ratingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateMovieRatingCommandHandler> _logger;

    public CreateMovieRatingCommandHandler(
        IMovieRepository movieRepository,
        IMovieRatingRepository ratingRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateMovieRatingCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _ratingRepository = ratingRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(CreateMovieRatingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Unauthorized rating attempt. MovieId: {MovieId}", request.MovieId);
            return BaseResponse.Fail("User not authenticated");
        }

        _logger.LogInformation("User {UserId} rating movie {MovieId} with {Rating}",
            userId, request.MovieId, request.Request.Rating);

        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning("Movie not found. MovieId: {MovieId}", request.MovieId);
            return BaseResponse.Fail("Movie not found");
        }

        var existing = await _ratingRepository
            .GetByMovieAndUserAsync(request.MovieId, userId, cancellationToken);

        if (existing is null)
        {
            _logger.LogInformation("Creating new rating. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId, userId);

            var rating = new MovieRating
            {
                MovieId = request.MovieId,
                UserId = userId,
                Rating = request.Request.Rating
            };

            await _ratingRepository.AddAsync(rating, cancellationToken);
            movie.RatingCount += 1;

        }
        else
        {
            _logger.LogInformation("Updating rating. MovieId: {MovieId}, UserId: {UserId}, Old: {Old}, New: {New}",
                request.MovieId, userId, existing.Rating, request.Request.Rating);

            existing.Rating = request.Request.Rating;
        }
        await _ratingRepository.SaveChangesAsync(cancellationToken);

        var average = await _ratingRepository
            .GetAverageRatingAsync(request.MovieId, cancellationToken);

        movie.UserAverageRating = Math.Round(average, 1);

        await _movieRepository.UpdateAsync(movie, cancellationToken);
        await _ratingRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated average rating. MovieId: {MovieId}, Avg: {Avg}",
            request.MovieId, movie.UserAverageRating);

        return BaseResponse.Ok("Movie rating saved successfully");
    }
}
