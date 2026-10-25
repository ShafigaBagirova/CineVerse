using Application.Common.Responses;
using Application.WatchRooms.Commands;
using Application.WatchRooms.Dtos;
using Application.WatchRooms.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WatchRoomController : ControllerBase
{
    private readonly IMediator _mediator;

    public WatchRoomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse<GetWatchRoomByIdResponse>>> Create([FromBody] CreateWatchRoomRequest request)
    {
        var result = await _mediator.Send(new CreateWatchRoomCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpPost("{code}/join")]
    public async Task<ActionResult<BaseResponse<GetWatchRoomByIdResponse>>> Join([FromRoute] string code)
    {
        var result = await _mediator.Send(new JoinWatchRoomCommand(code));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(
        [FromRoute] int id,
        [FromBody] UpdateWatchRoomRequest request)
    {
        var result = await _mediator.Send(new UpdateWatchRoomCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteWatchRoomCommand(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet("youtube/search")]
    public async Task<ActionResult<BaseResponse<List<SearchYouTubeVideosResponse>>>> SearchYouTube(
        [FromQuery] string query)
    {
        var result = await _mediator.Send(new SearchYouTubeVideosQuery(query));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet("youtube/popular")]
    public async Task<ActionResult<BaseResponse<GetPopularYouTubeVideosResponse>>> GetPopularYouTube(
        [FromQuery] string? pageToken)
    {
        var result = await _mediator.Send(new GetPopularYouTubeVideosQuery(pageToken));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetWatchRoomByIdResponse>>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetWatchRoomByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllWatchRoomsResponse>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = true)
    {
        var result = await _mediator.Send(new GetAllWatchRoomsQuery(pageNumber, pageSize, search, sortBy, desc));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
