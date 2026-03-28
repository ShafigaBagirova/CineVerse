using Application.AdminDashboard.Queries;
using Application.Common.Responses;
using MediatR;
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
    [HttpGet("summary")]
    public async Task<ActionResult<BaseResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAdminDashboardSummaryQuery(),
            cancellationToken);

        return Ok(result);
    }
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
}
