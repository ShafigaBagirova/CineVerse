using Application.Common.Responses;
using Application.Follows.Commands;
using Application.Follows.Dtos;
using Application.Follows.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class FollowController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("{followingId}")]
    public async Task<ActionResult<BaseResponse>> FollowUser([FromRoute] string followingId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new FollowUserCommand(followingId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [Authorize]
    [HttpDelete("{followingId}")]
    public async Task<ActionResult<BaseResponse>> UnfollowUser([FromRoute] string followingId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UnfollowUserCommand(followingId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [Authorize]
    [HttpGet("{userId}/status")]
    public async Task<ActionResult<BaseResponse<FollowStatusDto>>> GetFollowStatus(
    [FromRoute] string userId,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFollowStatusQuery(userId), cancellationToken);
        return result;
    }

    [HttpGet("{userId}/stats")]
    public async Task<ActionResult<BaseResponse<FollowStatsDto>>> GetFollowStats(
    [FromRoute] string userId,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFollowStatsQuery(userId), cancellationToken);
        return result;
    }
    [HttpGet("{userId}/followers")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<FollowUserItemDto>>>> GetFollowers(
    [FromRoute] string userId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetFollowersQuery(userId, page, pageSize),
            cancellationToken);

        return result;
    }
    [HttpGet("{userId}/followings")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<FollowUserItemDto>>>> GetFollowings(
    [FromRoute] string userId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetFollowingsQuery(userId, page, pageSize),
            cancellationToken);

        return result;
    }
    [Authorize]
    [HttpGet("{userId}/mutual-followings")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<FollowUserItemDto>>>> GetMutualFollowings(
    [FromRoute] string userId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMutualFollowingsQuery(userId, page, pageSize),
            cancellationToken);

        return result;
    }
    [Authorize(Policy = Policies.VipOnly)]
    [HttpGet("suggested-users")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<SuggestedUserItemDto>>>> GetSuggestedUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetSuggestedUsersQuery(page, pageSize),
            cancellationToken);

        return result;
    }
    [Authorize]
    [HttpGet("{userId}/relationship")]
    public async Task<ActionResult<BaseResponse<FollowRelationshipDto>>> GetFollowRelationship(
    [FromRoute] string userId,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetFollowRelationshipQuery(userId),
            cancellationToken);

        return result;
    }
}
