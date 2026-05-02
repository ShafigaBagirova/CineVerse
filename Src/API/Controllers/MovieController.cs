using Application.Common.Responses;
using Application.Movies.Commands;
using Application.Movies.Dtos;
using Application.Movies.Queries;
using Application.Validations.Movie;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MovieController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CineVerseDbContext _db;

    public MovieController(IMediator mediator, CineVerseDbContext db)
    {
        _mediator = mediator;
        _db = db;
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
  
        if (string.IsNullOrWhiteSpace(request.DirectorName))
            request.DirectorName = request.Director;

        if (string.IsNullOrWhiteSpace(request.ActorName))
            request.ActorName = request.Actor ?? request.Cast;

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
    [Authorize(Policy=Policies.Authenticated)]
    [HttpGet("suggested")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetSuggestedMoviesResponse>>>> GetSuggestedMovies(
    [FromQuery] GetSuggestedMoviesRequest request)
    {
        var result = await _mediator.Send(new GetSuggestedMoviesQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("debug-actor")]
    public async Task<ActionResult<object>> DebugActorData([FromQuery] string name = "Tim Robbins", CancellationToken cancellationToken = default)
    {
        var term = (name ?? string.Empty).Trim();
        var like = $"%{term}%";

        async Task<bool> TableExistsAsync(string tableName)
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table";
            var p = cmd.CreateParameter();
            p.ParameterName = "@table";
            p.Value = tableName;
            cmd.Parameters.Add(p);
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }

        var hasActorsTable = await TableExistsAsync("Actors");
        var hasMovieActorsTable = await TableExistsAsync("MovieActors");
        var hasDirectorsTable = await TableExistsAsync("Directors");
        var hasMovieDirectorsTable = await TableExistsAsync("MovieDirectors");

        var shawshank = await _db.Movies
            .AsNoTracking()
            .Where(m => EF.Functions.Like(m.Title, "%Shawshank%"))
            .Select(m => new { m.Id, m.Title, m.Director, m.Actors })
            .FirstOrDefaultAsync(cancellationToken);

        var actorsColumnMatches = await _db.Movies
            .AsNoTracking()
            .Where(m => m.Actors != null && EF.Functions.Like(m.Actors, like))
            .OrderBy(m => m.Title)
            .Select(m => new { m.Id, m.Title, m.Actors })
            .Take(20)
            .ToListAsync(cancellationToken);

        object relationSnapshot;
        if (hasActorsTable && hasMovieActorsTable)
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT TOP 20 a.Id AS ActorId, a.Name AS ActorName, ma.MovieId, m.Title
FROM Actors a
LEFT JOIN MovieActors ma ON ma.ActorId = a.Id
LEFT JOIN Movies m ON m.Id = ma.MovieId
WHERE a.Name LIKE @name
ORDER BY a.Name, m.Title";
            var p = cmd.CreateParameter();
            p.ParameterName = "@name";
            p.Value = like;
            cmd.Parameters.Add(p);

            var rows = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new
                {
                    ActorId = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                    ActorName = reader.IsDBNull(1) ? null : reader.GetString(1),
                    MovieId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                    MovieTitle = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            relationSnapshot = new
            {
                ActorRelationRows = rows.Count,
                Rows = rows
            };
        }
        else
        {
            relationSnapshot = new
            {
                ActorRelationRows = 0,
                Rows = Array.Empty<object>()
            };
        }

        return Ok(new
        {
            SearchName = term,
            Tables = new
            {
                HasActorsTable = hasActorsTable,
                HasMovieActorsTable = hasMovieActorsTable,
                HasDirectorsTable = hasDirectorsTable,
                HasMovieDirectorsTable = hasMovieDirectorsTable
            },
            ShawshankMovie = shawshank,
            ActorsColumn = new
            {
                MatchCount = actorsColumnMatches.Count,
                Matches = actorsColumnMatches
            },
            ActorRelation = relationSnapshot
        });
    }

}