using Application.Common.Interfaces;
using Application.Movies.Dtos;
using AutoMapper;
using MediatR;

namespace Application.Movies.Queries;

public sealed class GetMovieByIdQueryHandler
    : IRequestHandler<GetMovieByIdQuery, GetMovieByIdResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetMovieByIdQueryHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<GetMovieByIdResponse> Handle(
        GetMovieByIdQuery request,
        CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdWithMediaAsync(request.Id, cancellationToken);

        if (movie is null)
            throw new KeyNotFoundException("Movie could not be found.");

        return _mapper.Map<GetMovieByIdResponse>(movie);
    }
}
