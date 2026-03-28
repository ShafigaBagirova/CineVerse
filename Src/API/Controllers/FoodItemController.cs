using Application.Common.Responses;
using Application.FoodItems.Commands;
using Application.FoodItems.Dtos;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoodItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoodItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = Policies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create(
        [FromBody] CreateFoodItemRequest request)
    {
        var result = await _mediator.Send(new CreateFoodItemCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = Policies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(
        int id,
        [FromBody] UpdateFoodItemRequest request)
    {
        var result = await _mediator.Send(new UpdateFoodItemCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = Policies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteFoodItemCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
