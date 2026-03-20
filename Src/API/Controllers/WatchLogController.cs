using Application.Common.Responses;
using Application.Watched.Commands;
using Application.Watched.Dtos;
using Application.Watched.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WatchLogController : ControllerBase
{
    private readonly IMediator _mediator;

    public WatchLogController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpPost("{movieId:int}/watched")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> MarkAsWatched(
    [FromRoute] int movieId,
    CancellationToken cancellationToken)
    {
        var command = new MarkAsWatchedCommand(movieId);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{movieId:int}/watched")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> RemoveFromWatched(
        [FromRoute] int movieId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveFromWatchedCommand(movieId);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("watched/me")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<WatchedMovieDto>>>> GetMyWatchedMovies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyWatchedMoviesQuery(page, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}
