using Application.Common.Responses;
using Application.Reviews.Dtos;
using MediatR;

namespace Application.Reviews.Queries;

public record GetMyReviewQuery(int MovieId)
    : IRequest<BaseResponse<ReviewDto>>;
