using Application.Common.Responses;
using Application.Movies.Commands;
using Application.Movies.Dtos;
using Application.Movies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RatingController : ControllerBase
{
    private readonly IMediator _mediator;

    public RatingController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet("{movieId:int}/ratings/summary")]
    public async Task<ActionResult<BaseResponse<GetMovieRatingSummaryResponse>>> GetSummary(
        [FromRoute] int movieId,
        CancellationToken cancellationToken)
    {
        var query = new GetMovieRatingSummaryQuery(movieId);

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [Authorize]
    [HttpPost("{movieId:int}/ratings")]
    public async Task<ActionResult<BaseResponse>> CreateRating(int movieId,CreateMovieRatingRequest request)
    {
        var command = new CreateMovieRatingCommand(movieId, request);

        var response = await _mediator.Send(command);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}
