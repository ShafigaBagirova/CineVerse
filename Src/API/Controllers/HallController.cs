using Application.Common.Responses;
using Application.Halls.Commands;
using Application.Halls.Dtos;
using Application.Halls.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HallController : ControllerBase
{
    private readonly IMediator _mediator;

    public HallController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = Policies.ManageCinemas)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateHallRequest request)
    {
        var result = await _mediator.Send(new CreateHallCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.ManageCinemas)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(
    [FromRoute] int id,
    [FromBody] UpdateHallRequest request)
    {
        var result = await _mediator.Send(new UpdateHallCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.ManageCinemas)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteHallCommand(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetHallByIdResponse>>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetHallByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllHallsResponse>>>> GetAll(
        [FromQuery] GetAllHallsRequest request)
    {
        var result = await _mediator.Send(new GetAllHallsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
