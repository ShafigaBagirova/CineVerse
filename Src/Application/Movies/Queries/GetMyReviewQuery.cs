using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public record GetMyReviewQuery(int MovieId)
    : IRequest<BaseResponse<ReviewDto>>;
