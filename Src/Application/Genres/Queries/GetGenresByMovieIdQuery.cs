using Application.Movies.Dtos;
using MediatR;

namespace Application.Genres.Queries;

public sealed record GetGenresByMovieIdQuery(int MovieId)
    : IRequest<List<MovieGenreDto>>;