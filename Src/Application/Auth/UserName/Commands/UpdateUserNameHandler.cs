using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.UserName.Commands;

public sealed class UpdateUserNameHandler
    : IRequestHandler<UpdateUserNameCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;

    public UpdateUserNameHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<BaseResponse> Handle(UpdateUserNameCommand request, CancellationToken ct)
        => _identityService.UpdateUserNameAsync(request.UserId,request.NewUserName);
}
