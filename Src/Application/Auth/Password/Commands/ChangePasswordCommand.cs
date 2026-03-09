using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Password.Commands;

public sealed record ChangePasswordCommand(
    string UserId,
   string CurrentPassword,
    string NewPassword
) : IRequest<BaseResponse>;
