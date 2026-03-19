using Application.Common.Responses;
using Application.Movies.Commands;
using Application.Movies.Dtos;
using Application.Movies.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{movieId:int}/reviews")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<ReviewDto>>>> GetReviewsByMovie(
        [FromRoute] int movieId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReviewsByMovieQuery(movieId, page, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [HttpPost("{movieId:int}/reviews")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> CreateReview(
    [FromRoute] int movieId,
    [FromBody] CreateReviewRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(movieId, request);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{movieId:int}/reviews")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> UpdateReview(
        [FromRoute] int movieId,
        [FromBody] UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(movieId, request);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{movieId:int}/reviews")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> DeleteReview(
        [FromRoute] int movieId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand(movieId);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<ReviewDto>>>> GetAllReviews(
    CancellationToken cancellationToken)
    {
        var query = new GetAllReviewsQuery();

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [HttpGet("{movieId:int}/reviews/me")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse<ReviewDto>>> GetMyReview(
    [FromRoute] int movieId,
    CancellationToken cancellationToken)
    {
        var query = new GetMyReviewQuery(movieId);

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    [HttpDelete("reviews/{reviewId:int}")]
    [Authorize(Policy = Policies.ManageMovies)]
    public async Task<ActionResult<BaseResponse>> DeleteReviewByAdmin(
    [FromRoute] int reviewId,
    CancellationToken cancellationToken)
    {
        var command = new DeleteReviewByAdminCommand(reviewId);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}
