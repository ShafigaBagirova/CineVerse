using Application.Genres.Dtos;
using MediatR;

namespace Application.Genres.Queries;

public sealed record GetAllGenresQuery
    : IRequest<List<GetAllGenresResponse>>;
