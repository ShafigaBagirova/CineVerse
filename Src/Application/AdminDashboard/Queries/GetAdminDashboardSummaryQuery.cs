using Application.AdminDashboard.Dtos;
using MediatR;

namespace Application.AdminDashboard.Queries;

public sealed record GetAdminDashboardSummaryQuery()
    : IRequest<AdminDashboardSummaryDto>;
