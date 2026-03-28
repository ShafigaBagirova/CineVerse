using Application.Common.Responses;
using Application.FoodOrders.Commands;
using Application.FoodOrders.Dtos;
using Application.FoodOrders.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoodOrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoodOrderController(IMediator mediator)
    {
        _mediator = mediator;
    }
   
    [HttpPost("draft")]
    [Authorize(Policy =Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> CreateDraft(
        [FromBody] CreateFoodOrderDraftRequest request)
    {
        var result = await _mediator.Send(new CreateFoodOrderDraftCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("draft/{id:int}")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> UpdateDraft(
        int id,
        [FromBody] UpdateFoodOrderDraftRequest request)
    {
        var result = await _mediator.Send(new UpdateFoodOrderDraftCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> Cancel(int id)
    {
        var result = await _mediator.Send(new CancelFoodOrderCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize]
    [HttpGet("my-orders")]
    public async Task<ActionResult<BaseResponse<List<FoodOrderResponse>>>> GetMyOrders(
    [FromQuery] GetMyFoodOrdersRequest request)
    {
        var result = await _mediator.Send(new GetMyFoodOrdersQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<FoodOrderResponse>>>> GetAll(
    [FromQuery] GetAllFoodOrdersRequest request)
    {
        var result = await _mediator.Send(new GetAllFoodOrdersQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<FoodOrderResponse>>> GetById(int id)
    {
        var result = await _mediator.Send(new GetFoodOrderByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [Authorize(Roles = Policies.AdminOnly)]
    [HttpGet("summary")]
    public async Task<ActionResult<BaseResponse<FoodOrderSummaryResponse>>> GetSummary(
    [FromQuery] GetFoodOrderSummaryRequest request)
    {
        var result = await _mediator.Send(new GetFoodOrderSummaryQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Roles = Policies.AdminOnly)]
    [HttpGet("orders-by-day")]
    public async Task<ActionResult<BaseResponse<List<OrdersByDayResponse>>>> GetOrdersByDay(
    [FromQuery] GetFoodOrdersByDayRequest request)
    {
        var result = await _mediator.Send(new GetFoodOrdersByDayQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
