using MediatR;

namespace Application.Auth.Refresh.Commands;

public record CreateRefreshTokenResponse(
    string Token,
    DateTime ExpiresAtUtc
);
public record CreateRefreshTokenCommand(string UserId)
    : IRequest<CreateRefreshTokenResponse>;

