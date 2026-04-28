using Domain.Entities;
using MediatR;

namespace CineVerse.Application.Users.Commands;

public record CreateRefreshTokenCommand(User user, CancellationToken ct = default): IRequest<string>;