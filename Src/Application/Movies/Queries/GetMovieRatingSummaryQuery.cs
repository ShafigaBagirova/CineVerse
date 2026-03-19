using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public record GetMovieRatingSummaryQuery(int MovieId)
    : IRequest<BaseResponse<GetMovieRatingSummaryResponse>>;
