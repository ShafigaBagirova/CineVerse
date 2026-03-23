using Application.Common.Responses;
using Application.Movies.Commands;
using Application.Movies.Dtos;
using Application.Movies.Queries;
using Application.Validations.Movie;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MovieController : ControllerBase
{
    private readonly IMediator _mediator;

    public MovieController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = Policies.ManageMovies)]
    [HttpPost]
 public async Task<ActionResult<BaseResponse>> CreateMovie(
    [FromBody] CreateMovieCommand command,
    CancellationToken cancellationToken)
{
    var response = await _mediator.Send(command, cancellationToken);

    if (!response.Success)
        return BadRequest(response);

    return Ok(response);
}
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> UpdateMovie(
        [FromRoute] int id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMovieCommand(id, request);

        var response = await _mediator.Send(command, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> DeleteMovie(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMovieCommand (id);

        var response = await _mediator.Send(command, cancellationToken);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpPost("sync-tmdb")]
    public async Task<ActionResult<BaseResponse>> SyncMoviesFromTmdb(
       [FromQuery] int page,
       CancellationToken cancellationToken)
    {
        var command = new SyncMoviesFromTmdbCommand(page);

        var response = await _mediator.Send(command, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllMoviesResponse>>>> GetAllMMovies([FromQuery] GetAllMoviesRequest request)
    {
        var result = await _mediator.Send(
         new GetAllMoviesQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetMovieByIdResponse>> GetMovieById(
    [FromRoute] int id,
    CancellationToken cancellationToken)
    {
        var query = new GetMovieByIdQuery(id);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<GetMovieByIdResponse>> GetMovieBySlug(
    [FromRoute] string slug,
    CancellationToken cancellationToken)
    {
        var query = new GetMovieBySlugQuery(slug);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }

}