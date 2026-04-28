using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Commands;

public sealed class SubscribeVipCommandHandler(
    IIdentityService identityService,
    ILogger<SubscribeVipCommandHandler> logger)
    : IRequestHandler<SubscribeVipCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(SubscribeVipCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("SubscribeVip for UserId: {UserId}", request.UserId);
        return await identityService.SubscribeVipAsync(request.UserId, cancellationToken);
    }
}
