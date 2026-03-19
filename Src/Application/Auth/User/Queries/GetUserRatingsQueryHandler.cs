using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Queries;

public sealed class GetUserRatingsQueryHandler
    : IRequestHandler<GetUserRatingsQuery, BaseResponse<PaginatedResponse<UserRatingDto>>>
{
    private readonly IMovieRatingRepository _movieRatingRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserRatingsQueryHandler> _logger;

    public GetUserRatingsQueryHandler(
        IMovieRatingRepository movieRatingRepository,
        IMapper mapper,
        ILogger<GetUserRatingsQueryHandler> logger)
    {
        _movieRatingRepository = movieRatingRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<UserRatingDto>>> Handle(
        GetUserRatingsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetUserRatingsQuery started. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId,
            request.Page,
            request.PageSize);

        var totalCount = await _movieRatingRepository.GetCountByUserIdAsync(
            request.UserId,
            cancellationToken);

        var ratings = await _movieRatingRepository.GetPagedByUserIdAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var ratingDtos = _mapper.Map<List<UserRatingDto>>(ratings);

        var result = new PaginatedResponse<UserRatingDto>
        {
            Items = ratingDtos,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "GetUserRatingsQuery completed successfully. UserId: {UserId}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            request.UserId,
            ratingDtos.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<UserRatingDto>>.Ok(result);
    }
}