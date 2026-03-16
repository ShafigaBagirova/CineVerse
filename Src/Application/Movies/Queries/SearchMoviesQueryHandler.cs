using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class SearchMoviesQueryHandler
    : IRequestHandler<SearchMoviesQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchMoviesQueryHandler> _logger;

    public SearchMoviesQueryHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<SearchMoviesQueryHandler> logger)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        SearchMoviesQuery request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm.Trim();
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        _logger.LogInformation(
            "SearchMoviesQuery started. SearchTerm: {SearchTerm}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            searchTerm,
            pageNumber,
            pageSize);

        var totalCount = await _movieRepository.CountSearchAsync(searchTerm, cancellationToken);

        var movies = await _movieRepository.SearchPagedWithMediaAsync(
            searchTerm,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = _mapper.Map<List<GetAllMoviesResponse>>(movies);

        _logger.LogInformation(
            "SearchMoviesQuery completed. SearchTerm: {SearchTerm}, Returned: {Count}, TotalCount: {TotalCount}",
            searchTerm,
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