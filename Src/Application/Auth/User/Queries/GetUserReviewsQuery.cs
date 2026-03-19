using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Auth.User.Queries;

public record GetUserReviewsQuery(string UserId, int Page = 1, int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<ReviewDto>>>;
