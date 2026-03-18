using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetMoviesByUserRatingRangeQueryHandler
    : IRequestHandler<GetMoviesByUserRatingRangeQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMoviesByUserRatingRangeQueryHandler> _logger;

    public GetMoviesByUserRatingRangeQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<GetMoviesByUserRatingRangeQueryHandler> logger)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetMoviesByUserRatingRangeQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        _logger.LogInformation(
            "GetMoviesByUserRatingRangeQuery started. MinRating: {MinRating}, MaxRating: {MaxRating}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.MinRating,
            request.MaxRating,
            pageNumber,
            pageSize);

        var totalCount = await _movieRepository.CountByUserRatingRangeAsync(
            request.MinRating,
            request.MaxRating,
            cancellationToken);

        var movies = await _movieRepository.GetByUserRatingRangePagedAsync(
            request.MinRating,
            request.MaxRating,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = _mapper.Map<List<GetAllMoviesResponse>>(movies);

        _logger.LogInformation(
            "GetMoviesByUserRatingRangeQuery completed successfully. Returned {Count} items. TotalCount: {TotalCount}",
            items.Count,
            totalCount);

        return new PaginatedResponse<GetAllMoviesResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}