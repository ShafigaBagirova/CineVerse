using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.User.Queries;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, JwtUserInfoDto?>
{
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<JwtUserInfoDto?> Handle(GetCurrentUserQuery request, CancellationToken ct)
        => _identityService.GetUserInfoAsync(request.UserId);
}
