using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Email.Commands;

public sealed record UpdateEmailCommand(
    string UserId,
    string NewEmail
) : IRequest<BaseResponse>;
