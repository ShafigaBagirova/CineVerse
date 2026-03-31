using Application.AdminDashboard.Dtos;
using MediatR;

namespace Application.AdminDashboard.Queries;

public sealed record GetAdminRevenueChartQuery(int Days = 7)
    : IRequest<List<RevenueChartItemDto>>;