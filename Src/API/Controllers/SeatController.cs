using Application.Common.Responses;
using Application.Seats.Commands;
using Application.Seats.Dtos;
using Application.Seats.Queries;
using Application.Tickets.Dtos;
using Application.Tickets.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SeatController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeatController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Policy = Policies.ManageScreenings)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateSeatRequest request)
    {
        var result = await _mediator.Send(new CreateSeatCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.ManageScreenings)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(int id, [FromBody] UpdateSeatRequest request)
    {
        var result = await _mediator.Send(new UpdateSeatCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.ManageScreenings)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteSeatCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetSeatByIdResponse>>> GetById(int id)
    {
        var result = await _mediator.Send(new GetSeatByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllSeatsResponse>>>> GetAll(
    [FromQuery] GetAllSeatsRequest request)
    {
        var result = await _mediator.Send(new GetAllSeatsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{screeningId:int}/seats")]
    public async Task<ActionResult<BaseResponse<List<GetSeatsByScreeningResponse>>>> GetSeatsByScreening(int screeningId)
    {
        var result = await _mediator.Send(new GetSeatsByScreeningQuery(screeningId));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [AllowAnonymous]
    [HttpGet("{screeningId:int}/occupied-seats")]
    public async Task<ActionResult<BaseResponse<List<GetOccupiedSeatsByScreeningResponse>>>> GetOccupiedSeats(
       [FromRoute] int screeningId)
    {
        var result = await _mediator.Send(new GetOccupiedSeatsByScreeningQuery(screeningId));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{screeningId:int}/available-seats")]
    public async Task<ActionResult<BaseResponse<List<GetAvailableSeatsByScreeningResponse>>>> GetAvailableSeats(
        [FromRoute] int screeningId)
    {
        var result = await _mediator.Send(new GetAvailableSeatsByScreeningQuery(screeningId));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
