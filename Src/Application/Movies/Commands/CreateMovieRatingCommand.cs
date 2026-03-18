using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public record class CreateMovieRatingCommand(
    int MovieId,
    CreateMovieRatingRequest Request) : IRequest<BaseResponse>;