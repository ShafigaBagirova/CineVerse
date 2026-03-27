using Application.Common.Responses;
using Application.Tickets.Dtos;
using Application.Tickets.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = Policies.PurchaseTicket)]
    [HttpGet("my")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetMyTicketsResponse>>>> GetMyTickets(
        [FromQuery] GetMyTicketsRequest request)
    {
        var result = await _mediator.Send(new GetMyTicketsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.PurchaseTicket)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetTicketByIdResponse>>> GetTicketById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetTicketByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.ManageScreenings)]
    [HttpGet("screenings/{screeningId:int}")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetTicketsByScreeningResponse>>>> GetTicketsByScreening(
       [FromRoute] int screeningId,
       [FromQuery] GetTicketsByScreeningRequest request)
    {
        var result = await _mediator.Send(new GetTicketsByScreeningQuery(screeningId, request));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

}
