using Application.Common.Interfaces;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;

namespace Application.Movies.Queries;


public sealed class GetMovieBySlugQueryHandler
    : IRequestHandler<GetMovieBySlugQuery, GetMovieByIdResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetMovieBySlugQueryHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<GetMovieByIdResponse> Handle(GetMovieBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLower();

        var movie = await _movieRepository.GetBySlugWithMediaAsync(slug, cancellationToken);

        if (movie is null)
            throw new KeyNotFoundException("Movie tapılmadı.");

        return _mapper.Map<GetMovieByIdResponse>(movie);
    }
}
