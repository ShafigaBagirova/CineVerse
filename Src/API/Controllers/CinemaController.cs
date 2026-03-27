using Application.Cinemas.Commands;
using Application.Cinemas.Dtos;
using Application.Cinemas.Queries;
using Application.Common.Responses;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CinemaController : ControllerBase
{
    private readonly IMediator _mediator;

    public CinemaController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Policy = Policies.ManageCinemas)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateCinemaRequest request)
    {
        var result = await _mediator.Send(new CreateCinemaCommand(request));

        return result.Success ? Ok(result) : BadRequest(result);
    }
    [Authorize(Policy = Policies.ManageCinemas)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteCinemaCommand(id));

        return result.Success ? Ok(result) : NotFound(result);
    }
    [Authorize(Policy = Policies.ManageCinemas)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(
    [FromRoute] int id,
    [FromBody] UpdateCinemaRequest request)
    {
        var result = await _mediator.Send(new UpdateCinemaCommand(id, request));

        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllCinemasResponse>>>> GetAll([FromQuery] GetAllCinemasRequest request)
    {
        var result = await _mediator.Send(
            new GetAllCinemasQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetCinemaByIdResponse>>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetCinemaByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

}
