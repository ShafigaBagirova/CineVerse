using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;

namespace Application.Follows.Queries;

public sealed record GetFollowingsQuery(
    string UserId,
    int Page = 1,
    int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<FollowUserItemDto>>>;