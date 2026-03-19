using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetReviewsByMovieQueryHandler
    : IRequestHandler<GetReviewsByMovieQuery, BaseResponse<PaginatedResponse<ReviewDto>>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetReviewsByMovieQueryHandler> _logger;

    public GetReviewsByMovieQueryHandler(
        IReviewRepository reviewRepository,
        IIdentityService identityService,
        IMapper mapper,
        ILogger<GetReviewsByMovieQueryHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<BaseResponse<PaginatedResponse<ReviewDto>>> Handle(
        GetReviewsByMovieQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetReviewsByMovieQuery started for MovieId {MovieId}, Page {Page}, PageSize {PageSize}",
            request.MovieId,
            request.Page,
            request.PageSize);

        var totalCount = await _reviewRepository.GetCountByMovieIdAsync(
            request.MovieId,
            cancellationToken);

        var reviews = await _reviewRepository.GetPagedByMovieIdAsync(
            request.MovieId,
            request.Page,
            request.PageSize,
            cancellationToken);

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

        var result = new PaginatedResponse<ReviewDto>
        {
            Items = reviewDtos,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };

        _logger.LogInformation(
            "GetReviewsByMovieQuery completed successfully for MovieId {MovieId}. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            request.MovieId,
            reviewDtos.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<ReviewDto>>.Ok(result);
    }
}