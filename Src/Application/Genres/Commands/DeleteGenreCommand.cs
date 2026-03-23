using Application.Common.Responses;
using MediatR;

namespace Application.Genres.Commands;

public sealed record DeleteGenreCommand(
    int Id) : IRequest<BaseResponse>;