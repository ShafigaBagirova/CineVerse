using Application.Common.Responses;
using Application.WatchListItems.Commands;
using Application.WatchListItems.Dtos;
using Application.WatchListItems.Queries;
using Domain.Constants;
using Infrastructure.Persistence.Context;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WatchListItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public WatchListItemController(IMediator mediator)
    {
        _mediator = mediator;
    }
   
    [HttpPost("{movieId:int}/watchlist")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> AddToWatchlist(
     [FromRoute] int movieId,
     CancellationToken cancellationToken)
    {
        var command = new AddToWatchlistCommand(movieId);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [HttpDelete("{movieId:int}/watchlist")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> RemoveFromWatchlist(
    [FromRoute] int movieId,
    CancellationToken cancellationToken)
    {
        var command = new RemoveFromWatchlistCommand(movieId);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [HttpGet("watchlist/me")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<WatchlistMovieDto>>>> GetMyWatchlist(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new GetMyWatchlistQuery(page, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}
