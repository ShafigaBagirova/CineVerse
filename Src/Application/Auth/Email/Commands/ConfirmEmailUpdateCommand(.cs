using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Email.Commands;

public sealed record ConfirmUpdateEmailCommand(
    string UserId,
    string NewEmail,
    string Code
) : IRequest<BaseResponse>;
