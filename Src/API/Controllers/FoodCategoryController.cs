using Application.Common.Responses;
using Application.FoodCategories.Commands;
using Application.FoodCategories.Dtos;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoodCategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoodCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = Policies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create(
        [FromBody] CreateFoodCategoryRequest request)
    {
        var result = await _mediator.Send(new CreateFoodCategoryCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [Authorize(Roles = Policies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(
        int id,
        [FromBody] UpdateFoodCategoryRequest request)
    {
        var result = await _mediator.Send(new UpdateFoodCategoryCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = Policies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteFoodCategoryCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
