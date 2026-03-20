using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Follows.Queries;

public sealed record GetMutualFollowingsQuery(
    string UserId,
    int Page = 1,
    int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<FollowUserItemDto>>>;