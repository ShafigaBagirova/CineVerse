using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Validations.Movie;

public sealed class DeleteReviewCommandHandler
    : IRequestHandler<DeleteReviewCommand, BaseResponse>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(
        IReviewRepository reviewRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteReview started. MovieId: {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "DeleteReview failed. Authenticated user not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Authenticated user could not be found.");
        }

        var review = await _reviewRepository.GetByMovieAndUserAsync(
            request.MovieId,
            userId,
            cancellationToken);

        if (review is null)
        {
            _logger.LogWarning(
                "DeleteReview failed. Review not found. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId,
                userId);

            return BaseResponse.Fail("Review could not be found.");
        }

        review.IsDeleted = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DeleteReview completed successfully. ReviewId: {ReviewId}, MovieId: {MovieId}, UserId: {UserId}",
            review.Id,
            review.MovieId,
            review.UserId);

        return BaseResponse.Ok("Review deleted successfully.");
    }
}
