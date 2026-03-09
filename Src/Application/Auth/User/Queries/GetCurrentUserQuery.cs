using Application.Auth.Login.Dtos;
using MediatR;

namespace Application.Auth.User.Queries;

public sealed record GetCurrentUserQuery(string UserId)
    : IRequest<JwtUserInfoDto?>;