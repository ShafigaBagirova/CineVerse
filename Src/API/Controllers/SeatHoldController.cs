using Application.Common.Responses;
using Application.SeatHolds.Commands;
using Application.SeatHolds.Dtos;
using Application.SeatHolds.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SeatHoldController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeatHoldController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Policy = Policies.Authenticated)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse<GetSeatHoldByIdResponse>>> Create([FromBody] CreateSeatHoldRequest request)
    {
        var result = await _mediator.Send(new CreateSeatHoldCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.Authenticated)]
    [HttpPut("{id:int}/release")]
    public async Task<ActionResult<BaseResponse>> Release([FromRoute] int id)
    {
        var result = await _mediator.Send(new ReleaseSeatHoldCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetSeatHoldByIdResponse>>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetSeatHoldByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllSeatHoldsResponse>>>> GetAll(
    [FromQuery] GetAllSeatHoldsRequest request)
    {
        var result = await _mediator.Send(new GetAllSeatHoldsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet("screenings/{screeningId:int}")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetSeatHoldsByScreeningResponse>>>> GetByScreening(
    [FromRoute] int screeningId,
    [FromQuery] GetSeatHoldsByScreeningRequest request)
    {
        var result = await _mediator.Send(new GetSeatHoldsByScreeningQuery(screeningId, request));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

}
