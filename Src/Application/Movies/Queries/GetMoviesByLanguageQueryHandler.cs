using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;

namespace Application.Movies.Queries;

public sealed class GetMoviesByLanguageQueryHandler
    : IRequestHandler<GetMoviesByLanguageQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetMoviesByLanguageQueryHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetMoviesByLanguageQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language.Trim();
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var totalCount = await _movieRepository.CountByLanguageAsync(language, cancellationToken);
        var movies = await _movieRepository.GetByLanguagePagedWithMediaAsync(
            language,
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
