using Application.AdminDashboard.Dtos;
using MediatR;

namespace Application.AdminDashboard.Queries;

public sealed record GetAdminRecentPaymentsQuery(int Take = 10)
    : IRequest<List<RecentPaymentDto>>;