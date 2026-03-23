using Application.Common.Responses;
using Application.Genres.Dtos;
using MediatR;

namespace Application.Genres.Commands;

public sealed record CreateGenreCommand(
    CreateGenreRequest Request) : IRequest<BaseResponse>;