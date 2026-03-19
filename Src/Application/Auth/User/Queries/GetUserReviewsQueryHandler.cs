using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Queries;

public sealed class GetUserReviewsQueryHandler
    : IRequestHandler<GetUserReviewsQuery, BaseResponse<PaginatedResponse<ReviewDto>>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserReviewsQueryHandler> _logger;

    public GetUserReviewsQueryHandler(
        IReviewRepository reviewRepository,
        IIdentityService identityService,
        IMapper mapper,
        ILogger<GetUserReviewsQueryHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<ReviewDto>>> Handle(
        GetUserReviewsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetUserReviewsQuery started. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId,
            request.Page,
            request.PageSize);

        var totalCount = await _reviewRepository.GetCountByUserIdAsync(
            request.UserId,
            cancellationToken);

        var reviews = await _reviewRepository.GetPagedByUserIdAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);

        var userNames = await _identityService.GetUserNamesByIdsAsync(new[] { request.UserId });

        foreach (var reviewDto in reviewDtos)
        {
            if (userNames.TryGetValue(reviewDto.UserId, out var userName))
            {
                reviewDto.UserName = userName;
            }
        }

        var result = new PaginatedResponse<ReviewDto>
        {
            Items = reviewDtos,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "GetUserReviewsQuery completed successfully. UserId: {UserId}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            request.UserId,
            reviewDtos.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<ReviewDto>>.Ok(result);
    }
}