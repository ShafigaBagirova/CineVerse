using Application.Auth.User.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.User.Queries;

public sealed record GetUsersQuery(GetUsersRequest Request)
    : IRequest<PaginatedResponse<UserProfileDto>>;