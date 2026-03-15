using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;

namespace Application.Movies.Queries;

public sealed class GetMoviesByStatusQueryHandler
    : IRequestHandler<GetMoviesByStatusQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetMoviesByStatusQueryHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetMoviesByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var totalCount = await _movieRepository.CountByStatusAsync(request.Status, cancellationToken);
        var movies = await _movieRepository.GetByStatusPagedWithMediaAsync(
            request.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = _mapper.Map<List<GetAllMoviesResponse>>(movies);

        return new PaginatedResponse<GetAllMoviesResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}