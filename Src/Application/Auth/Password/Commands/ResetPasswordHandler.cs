using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Password.Commands;

public sealed class ResetPasswordHandler
    : IRequestHandler<ResetPasswordCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<BaseResponse> Handle(ResetPasswordCommand request, CancellationToken ct)
        => _identityService.ResetPasswordAsync(request.Request);
}