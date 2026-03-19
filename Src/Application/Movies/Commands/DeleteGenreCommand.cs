using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public sealed record DeleteGenreCommand(
    int Id) : IRequest<BaseResponse>;