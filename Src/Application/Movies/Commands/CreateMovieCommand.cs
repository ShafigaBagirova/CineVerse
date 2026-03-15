using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public record class CreateMovieCommand(CreateMovieRequest CreateMovieRequest) : IRequest<BaseResponse>;

