using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetMovieRatingSummaryQueryHandler
    : IRequestHandler<GetMovieRatingSummaryQuery, BaseResponse<GetMovieRatingSummaryResponse>>
{
    private readonly IMovieRatingRepository _movieRatingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMovieRatingSummaryQueryHandler> _logger;

    public GetMovieRatingSummaryQueryHandler(
        IMovieRatingRepository movieRatingRepository,
        ICurrentUserService currentUserService,
        ILogger<GetMovieRatingSummaryQueryHandler> logger)
    {
        _movieRatingRepository = movieRatingRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse<GetMovieRatingSummaryResponse>> Handle(
        GetMovieRatingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetMovieRatingSummary started for MovieId {MovieId}", request.MovieId);

        var average = await _movieRatingRepository
            .GetAverageRatingAsync(request.MovieId, cancellationToken);

        var count = await _movieRatingRepository
            .GetRatingsCountAsync(request.MovieId, cancellationToken);
        if (count == 0 && average is null)
        {
            return BaseResponse<GetMovieRatingSummaryResponse>
                .Fail("No ratings found for this movie.");
        }


        decimal? myRating = null;

        var userId = _currentUserService.UserId;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var rating = await _movieRatingRepository
                .GetByMovieAndUserAsync(request.MovieId, userId, cancellationToken);

            myRating = rating?.Rating;
        }

        var result = new GetMovieRatingSummaryResponse
        {
            UserAverageRating = average,
            RatingCount = count,
            MyRating = myRating
        };

        return BaseResponse<GetMovieRatingSummaryResponse>.Ok(result);
    }
}