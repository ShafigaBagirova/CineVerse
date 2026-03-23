using Application.Common.Interfaces;
using Application.Movies.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Genres.Queries;

public sealed class GetGenresByMovieIdQueryHandler
    : IRequestHandler<GetGenresByMovieIdQuery, List<MovieGenreDto>>
{
    private readonly IMovieGenreRepository _movieGenreRepository;
    private readonly ILogger<GetGenresByMovieIdQueryHandler> _logger;

    public GetGenresByMovieIdQueryHandler(
        IMovieGenreRepository movieGenreRepository,
        ILogger<GetGenresByMovieIdQueryHandler> logger)
    {
        _movieGenreRepository = movieGenreRepository;
        _logger = logger;
    }

    public async Task<List<MovieGenreDto>> Handle(
        GetGenresByMovieIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetGenresByMovieId started. MovieId: {MovieId}",
            request.MovieId);

        var movieGenres = await _movieGenreRepository
            .GetByMovieIdAsync(request.MovieId, cancellationToken);

        var response = movieGenres.Select(mg => new MovieGenreDto
        {
            GenreId = mg.GenreId,
            Name = mg.Genre.Name,
            IsPrimary = mg.IsPrimary,
            Order = mg.Order
        }).ToList();

        _logger.LogInformation(
            "GetGenresByMovieId completed. Count: {Count}",
            response.Count);

        return response;
    }
}