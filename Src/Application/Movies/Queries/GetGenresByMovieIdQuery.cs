using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetGenresByMovieIdQuery(int MovieId)
    : IRequest<List<MovieGenreDto>>;