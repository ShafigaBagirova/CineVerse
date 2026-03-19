using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class UpdateReviewCommandHandler
    : IRequestHandler<UpdateReviewCommand, BaseResponse>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateReviewCommandHandler> _logger;

    public UpdateReviewCommandHandler(
        IReviewRepository reviewRepository,
        ICurrentUserService currentUserService,
        ILogger<UpdateReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        UpdateReviewCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "UpdateReview started. MovieId: {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "UpdateReview failed. Authenticated user not found. MovieId: {MovieId}",
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
                "UpdateReview failed. Review not found. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId,
                userId);

            return BaseResponse.Fail("Review could not be found.");
        }

        var content = request.Request.Content.Trim();

        if (string.Equals(review.Content, content, StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "UpdateReview skipped. Review content is unchanged. ReviewId: {ReviewId}, MovieId: {MovieId}",
                review.Id,
                request.MovieId);

            return BaseResponse.Ok("No changes were made to the review.");
        }

        review.Content = content;
        review.IsEdited = true;
        review.UpdatedAt = DateTime.UtcNow;
        review.IsSpoiler = request.Request.IsSpoiler;

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UpdateReview completed successfully. ReviewId: {ReviewId}, MovieId: {MovieId}, UserId: {UserId}",
            review.Id,
            review.MovieId,
            review.UserId);

        return BaseResponse.Ok("Review updated successfully.");
    }
}