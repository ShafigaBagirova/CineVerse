using Application.AdminDashboard.Queries;
using Application.Common.Responses;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Policy =Policies.AdminOnly)]
    [HttpGet("summary")]
    public async Task<ActionResult<BaseResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAdminDashboardSummaryQuery(),
            cancellationToken);

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpGet("revenue-chart")]
    public async Task<ActionResult<BaseResponse>> GetRevenueChart(
       [FromQuery] int days = 7,
       CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminRevenueChartQuery(days),
            cancellationToken);

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpGet("top-movies")]
    public async Task<ActionResult<BaseResponse>> GetTopMovies(
    [FromQuery] int take = 5,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminTopMoviesQuery(take),
            cancellationToken);

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpGet("recent-payments")]
    public async Task<ActionResult<BaseResponse>> GetRecentPayments(
    [FromQuery] int take = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminRecentPaymentsQuery(take),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("recent-users")]
    public async Task<ActionResult<BaseResponse>> GetRecentUsers(
    [FromQuery] int take = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminRecentUsersQuery(take),
            cancellationToken);

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpGet("screening-occupancy")]
    public async Task<ActionResult<BaseResponse>> GetScreeningOccupancy(
    [FromQuery] int take = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminScreeningOccupancyQuery(take),
            cancellationToken);

        return Ok(result);
    }
}
