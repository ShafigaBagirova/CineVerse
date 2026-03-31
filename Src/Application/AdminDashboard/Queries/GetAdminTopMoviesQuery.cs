using Application.AdminDashboard.Dtos;
using MediatR;

namespace Application.AdminDashboard.Queries;

public sealed record GetAdminTopMoviesQuery(int Take = 5)
    : IRequest<List<TopMovieDto>>;