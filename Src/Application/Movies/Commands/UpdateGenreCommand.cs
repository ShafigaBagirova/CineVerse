using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public sealed record UpdateGenreCommand(
    int Id,
    UpdateGenreRequest Request) : IRequest<BaseResponse>;
