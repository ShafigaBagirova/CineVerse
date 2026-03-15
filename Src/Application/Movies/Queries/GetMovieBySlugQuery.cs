using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public record GetMovieBySlugQuery(string Slug) : IRequest<GetMovieByIdResponse>;
