using Application.Common.Responses;
using MediatR;

namespace Application.Follows.Commands;

public sealed record UnfollowUserCommand(string FollowingId) : IRequest<BaseResponse>;
