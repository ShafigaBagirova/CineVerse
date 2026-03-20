using Application.Common.Responses;
using MediatR;

namespace Application.Follows.Commands;

public sealed record FollowUserCommand(string FollowingId) : IRequest<BaseResponse>;