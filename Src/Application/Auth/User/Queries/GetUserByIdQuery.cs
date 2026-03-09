using Application.Auth.User.Dtos;
using MediatR;

namespace Application.Auth.User.Queries;

public sealed record GetUserByIdQuery(string UserId)
    : IRequest<UserProfileDto?>;