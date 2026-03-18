using Application.Common.Responses;
using Application.MovieRatings.Commands;
using Application.MovieRatings.Dtos;
using Application.Movies.Commands;
using Application.Movies.Dtos;
using Application.Movies.Queries;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

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
    public async Task<ActionResult<PaginatedResponse<GetAllMoviesResponse>>> GetAllMovies(
    [FromQuery] GetAllMoviesQuery query,
    CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
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
    [HttpGet("language/{language}")]
    public async Task<ActionResult<PaginatedResponse<GetAllMoviesResponse>>> GetMoviesByLanguage(
    [FromRoute] string language,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new GetMoviesByLanguageQuery(language, pageNumber, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [HttpGet("status/{status}")]
    public async Task<ActionResult<PaginatedResponse<GetAllMoviesResponse>>> GetMoviesByStatus(
    [FromRoute] MovieStatus status,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new GetMoviesByStatusQuery(status, pageNumber, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [HttpGet("rating-range")]
    public async Task<ActionResult<PaginatedResponse<GetAllMoviesResponse>>> GetMoviesByUserRatingRange(
    [FromQuery] decimal minRating,
    [FromQuery] decimal maxRating,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new GetMoviesByUserRatingRangeQuery(minRating, maxRating, pageNumber, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [HttpGet("year/{year:int}")]
    public async Task<ActionResult<PaginatedResponse<GetAllMoviesResponse>>> GetMoviesByYear(
    [FromRoute] int year,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new GetMoviesByYearQuery(year, pageNumber, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<GetAllMoviesResponse>>> SearchMovies(
    [FromQuery] string searchTerm,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new SearchMoviesQuery(searchTerm, pageNumber, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [HttpGet("tmdb-rating-range")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<GetAllMoviesResponse>>> GetMoviesByTmdbRatingRange(
    [FromQuery] decimal minRating,
    [FromQuery] decimal maxRating,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new GetMoviesByTmdbRatingRangeQuery(minRating, maxRating, pageNumber, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [Authorize]
    [HttpPost("{movieId:int}/ratings")]
    public async Task<ActionResult<BaseResponse>> CreateRating(
    int movieId,
    CreateMovieRatingRequest request)
    {
        var command = new CreateMovieRatingCommand(movieId, request);

        var response = await _mediator.Send(command);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}