using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Movies.Queries;


public sealed class GetAllMoviesQueryHandler
    : IRequestHandler<GetAllMoviesQuery, PaginatedResponse<GetAllMoviesResponse>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetAllMoviesQueryHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<GetAllMoviesResponse>> Handle(
        GetAllMoviesQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var totalCount = await _movieRepository.CountAsync(cancellationToken);
        var movies = await _movieRepository.GetPagedWithMediaAsync(pageNumber,pageSize,request.SortBy,cancellationToken);

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