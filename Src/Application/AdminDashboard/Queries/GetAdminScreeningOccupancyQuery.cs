using Application.AdminDashboard.Dtos;
using MediatR;

namespace Application.AdminDashboard.Queries;

public sealed record GetAdminScreeningOccupancyQuery(int Take = 10)
    : IRequest<List<ScreeningOccupancyDto>>;