using Application.AdminDashboard.Dtos;
using MediatR;

namespace Application.AdminDashboard.Queries;

public sealed record GetAdminRecentUsersQuery(int Take = 10)
    : IRequest<List<RecentUserDto>>;