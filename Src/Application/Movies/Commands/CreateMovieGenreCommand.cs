using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public record CreateMovieGenreCommand(
    int MovieId,
    CreateMovieGenreRequest Request)
    : IRequest<BaseResponse>;
