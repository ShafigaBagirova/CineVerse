using Application.Auth.User.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.User.Queries;

public record GetUserRatingsQuery(string UserId, int Page = 1, int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<UserRatingDto>>>;
