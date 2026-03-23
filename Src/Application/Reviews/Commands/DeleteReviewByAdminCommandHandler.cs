using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Reviews.Commands;

public sealed class DeleteReviewByAdminCommandHandler
    : IRequestHandler<DeleteReviewByAdminCommand, BaseResponse>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<DeleteReviewByAdminCommandHandler> _logger;

    public DeleteReviewByAdminCommandHandler(
        IReviewRepository reviewRepository,
        ILogger<DeleteReviewByAdminCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        DeleteReviewByAdminCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteReviewByAdmin started. ReviewId: {ReviewId}",
            request.ReviewId);

        var review = await _reviewRepository.GetByIdAsync(
            request.ReviewId,
            cancellationToken);

        if (review is null)
        {
            _logger.LogWarning(
                "DeleteReviewByAdmin failed. Review not found. ReviewId: {ReviewId}",
                request.ReviewId);

            return BaseResponse.Fail("Review could not be found.");
        }

        review.IsDeleted = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DeleteReviewByAdmin completed successfully. ReviewId: {ReviewId}, MovieId: {MovieId}, UserId: {UserId}",
            review.Id,
            review.MovieId,
            review.UserId);

        return BaseResponse.Ok("Review deleted successfully.");
    }
}