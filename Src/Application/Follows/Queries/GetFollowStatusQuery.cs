using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;

namespace Application.Follows.Queries;

public sealed record GetFollowStatusQuery(string UserId)
    : IRequest<BaseResponse<FollowStatusDto>>;