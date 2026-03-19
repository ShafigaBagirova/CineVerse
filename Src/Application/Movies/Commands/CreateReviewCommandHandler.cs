using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class CreateReviewCommandHandler
    : IRequestHandler<CreateReviewCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        IMovieRepository movieRepository,
        IReviewRepository reviewRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "CreateReview started. MovieId: {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "CreateReview failed. Authenticated user not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Authenticated user could not be found.");
        }

        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning(
                "CreateReview failed. Movie not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Movie could not be found.");
        }

        var existingReview = await _reviewRepository.GetByMovieAndUserAsync(
            request.MovieId,
            userId,
            cancellationToken);

        if (existingReview is not null)
        {
            _logger.LogWarning(
                "CreateReview failed. Review already exists. MovieId: {MovieId}, UserId: {UserId}, ReviewId: {ReviewId}",
                request.MovieId,
                userId,
                existingReview.Id);

            return BaseResponse.Fail("You have already reviewed this movie.");
        }

        var content = request.Request.Content.Trim();
        var isspoiled= request.Request.IsSpoiler;
        var review = new Review
        {
            MovieId = request.MovieId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsEdited = false,
            IsDeleted = false,
            IsSpoiler= isspoiled
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreateReview completed successfully. ReviewId: {ReviewId}, MovieId: {MovieId}, UserId: {UserId}",
            review.Id,
            review.MovieId,
            review.UserId);

        return BaseResponse.Ok("Review created successfully.");
    }
}