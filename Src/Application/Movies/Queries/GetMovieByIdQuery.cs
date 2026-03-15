using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetMovieByIdQuery(int Id) : IRequest<GetMovieByIdResponse>;
