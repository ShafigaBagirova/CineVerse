using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Reviews.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Reviews.Queries;

public sealed class GetMyReviewQueryHandler
    : IRequestHandler<GetMyReviewQuery, BaseResponse<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyReviewQueryHandler> _logger;

    public GetMyReviewQueryHandler(
        IReviewRepository reviewRepository,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IMapper mapper,
        ILogger<GetMyReviewQueryHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<ReviewDto>> Handle(
        GetMyReviewQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetMyReviewQuery started for MovieId {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "GetMyReviewQuery failed. Authenticated user not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse<ReviewDto>.Fail("Authenticated user could not be found.");
        }

        var review = await _reviewRepository.GetByMovieAndUserAsync(
            request.MovieId,
            userId,
            cancellationToken);

        if (review is null)
        {
            _logger.LogWarning(
                "GetMyReviewQuery failed. Review not found. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId,
                userId);

            return BaseResponse<ReviewDto>.Fail("Review could not be found.");
        }

        var reviewDto = _mapper.Map<ReviewDto>(review);

        var userNames = await _identityService.GetUserNamesByIdsAsync(new[] { review.UserId });

        if (userNames.TryGetValue(review.UserId, out var userName))
        {
            reviewDto.UserName = userName;
        }

        _logger.LogInformation(
            "GetMyReviewQuery completed successfully. ReviewId: {ReviewId}, MovieId: {MovieId}, UserId: {UserId}",
            review.Id,
            review.MovieId,
            review.UserId);

        return BaseResponse<ReviewDto>.Ok(reviewDto);
    }
}