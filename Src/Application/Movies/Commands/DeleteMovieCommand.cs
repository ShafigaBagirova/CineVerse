using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public record DeleteMovieCommand(int Id) : IRequest<BaseResponse>;
