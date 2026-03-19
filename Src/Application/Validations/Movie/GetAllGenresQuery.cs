using Application.Movies.Dtos;
using MediatR;

namespace Application.Validations.Movie;

public sealed record GetAllGenresQuery
    : IRequest<List<GetAllGenresResponse>>;
