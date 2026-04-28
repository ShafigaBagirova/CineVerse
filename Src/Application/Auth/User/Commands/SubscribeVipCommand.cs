using Application.Common.Responses;
using MediatR;

namespace Application.Auth.User.Commands;

public sealed record SubscribeVipCommand(string UserId) : IRequest<BaseResponse>;
