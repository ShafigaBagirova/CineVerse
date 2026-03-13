using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.UserName.Commands;

public sealed class UpdateUserNameHandler
    : IRequestHandler<UpdateUserNameCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<UpdateUserNameHandler> _logger;

    public UpdateUserNameHandler(
        IIdentityService identityService,
        ILogger<UpdateUserNameHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(UpdateUserNameCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Updating user name. UserId: {UserId}, NewUserName: {NewUserName}",
            request.UserId,
            request.NewUserName);

        var response = await _identityService.UpdateUserNameAsync(request.UserId, request.NewUserName);

        if (response.Success)
        {
            _logger.LogInformation(
                "User name updated successfully. UserId: {UserId}",
                request.UserId);
        }
        else
        {
            _logger.LogWarning(
                "User name update failed. UserId: {UserId}, Message: {Message}",
                request.UserId,
                response.Message);
        }

        return response;
    }
}
