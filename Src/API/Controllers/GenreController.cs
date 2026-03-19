using Application.Common.Responses;
using Application.Movies.Commands;
using Application.Movies.Dtos;
using Application.Validations.Movie;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GenreController : ControllerBase
{
    private readonly IMediator _mediator;

    public GenreController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpDelete("{movieId:int}/genres/{genreId:int}")]
    public async Task<ActionResult<BaseResponse>> DeleteMovieGenre(
    [FromRoute] int movieId,
    [FromRoute] int genreId,
   CancellationToken cancellationToken)
    {
        var command = new DeleteMovieGenreCommand(movieId, genreId);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpPut("{movieId:int}/genres/{genreId:int}")]
    public async Task<ActionResult<BaseResponse>> UpdateMovieGenre(
        [FromRoute] int movieId,
        [FromRoute] int genreId,
        [FromBody] UpdateMovieGenreRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMovieGenreCommand(movieId, genreId, request);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpPost("{movieId:int}/genres")]
    public async Task<ActionResult<BaseResponse>> CreateMovieGenre(
    [FromRoute] int movieId,
    [FromBody] CreateMovieGenreRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CreateMovieGenreCommand(movieId, request);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> CreateGenre(
    [FromBody] CreateGenreRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CreateGenreCommand(request);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> UpdateGenre(
        [FromRoute] int id,
        [FromBody] UpdateGenreRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGenreCommand(id, request);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [Authorize(Policy = Policies.ManageMovies)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> DeleteGenre(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteGenreCommand(id);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [HttpGet]
    public async Task<ActionResult<List<GetAllGenresResponse>>> GetAllGenres(
    CancellationToken cancellationToken)
    {
        var query = new GetAllGenresQuery();

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    [HttpPost("sync-tmdb")]
    [Authorize(Policy = Policies.ManageMovies)]
    public async Task<ActionResult<BaseResponse>> SyncGenresFromTmdb(
       CancellationToken cancellationToken)
    {
        var command = new SyncGenresFromTmdbCommand();

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}
