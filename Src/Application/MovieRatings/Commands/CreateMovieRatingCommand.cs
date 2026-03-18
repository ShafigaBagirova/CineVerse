using Application.Common.Responses;
using Application.MovieRatings.Dtos;
using MediatR;

namespace Application.MovieRatings.Commands;

public record class CreateMovieRatingCommand(
    int MovieId,
    CreateMovieRatingRequest Request) : IRequest<BaseResponse>;