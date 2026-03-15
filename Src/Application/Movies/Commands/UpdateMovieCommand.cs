using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public record UpdateMovieCommand(int Id,UpdateMovieRequest UpdateMovieRequest) : IRequest<BaseResponse>;

