using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Password.Commands;

public sealed class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;

    public ChangePasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<BaseResponse> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        return await _identityService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword
        );
    }
}
