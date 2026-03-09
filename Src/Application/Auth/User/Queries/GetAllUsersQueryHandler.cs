using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.User.Queries;

public sealed class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, List<UserProfileDto>>
{
    private readonly IIdentityService _identityService;

    public GetAllUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<List<UserProfileDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
        => _identityService.GetAllUsersAsync();
}
