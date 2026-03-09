using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.User.Queries;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserProfileDto?>
{
    private readonly IIdentityService _identityService;

    public GetUserByIdQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<UserProfileDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
        => _identityService.GetUserByIdAsync(request.UserId);
}