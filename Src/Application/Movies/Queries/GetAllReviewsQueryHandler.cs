using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetAllReviewsQueryHandler
    : IRequestHandler<GetAllReviewsQuery, BaseResponse<List<ReviewDto>>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllReviewsQueryHandler> _logger;

    public GetAllReviewsQueryHandler(
        IReviewRepository reviewRepository,
        IIdentityService identityService,
        IMapper mapper,
        ILogger<GetAllReviewsQueryHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<List<ReviewDto>>> Handle(
        GetAllReviewsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAllReviewsQuery started.");

        var reviews = await _reviewRepository.GetAllAsync(cancellationToken);

        var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);

        var userIds = reviewDtos
            .Select(x => x.UserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var userNames = await _identityService.GetUserNamesByIdsAsync(userIds);

        foreach (var reviewDto in reviewDtos)
        {
            if (userNames.TryGetValue(reviewDto.UserId, out var userName))
            {
                reviewDto.UserName = userName;
            }
        }

        _logger.LogInformation(
            "GetAllReviewsQuery completed successfully. ReviewCount: {ReviewCount}",
            reviewDtos.Count);

        return BaseResponse<List<ReviewDto>>.Ok(reviewDtos);
    }
}
