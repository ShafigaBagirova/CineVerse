using Domain.Entities;
using MediatR;

namespace CineVerse.Application.Users.Commands;

public record  RefreshTokenValidateAndConsumeCommand(string token, CancellationToken ct = default) : IRequest<User?>;

