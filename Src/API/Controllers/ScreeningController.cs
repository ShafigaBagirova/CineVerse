using Application.Common.Responses;
using Application.Screenings.Commands;
using Application.Screenings.Dtos;
using Application.Screenings.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScreeningController : ControllerBase
{
    private readonly IMediator _mediator;

    public ScreeningController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(int id, [FromBody] UpdateScreeningRequest request)
    {
        var result = await _mediator.Send(new UpdateScreeningCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateScreeningRequest request)
    {
        var result = await _mediator.Send(new CreateScreeningCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteScreeningCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetScreeningByIdResponse>>> GetById(int id)
    {
        var result = await _mediator.Send(new GetScreeningByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllScreeningsResponse>>>> GetAll(
    [FromQuery] GetAllScreeningsRequest request)
    {
        var result = await _mediator.Send(new GetAllScreeningsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
