using Application.Common.Responses;
using Application.FoodCategories.Commands;
using Application.FoodCategories.Dtos;
using Application.FoodCategories.Queries;
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

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create(
        [FromBody] CreateFoodCategoryRequest request)
    {
        var result = await _mediator.Send(new CreateFoodCategoryCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [Authorize(Policy = Policies.AdminOnly)]
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

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteFoodCategoryCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<FoodCategoryResponse>>>> GetAll(
    [FromQuery] GetAllFoodCategoriesRequest request)
    {
        var result = await _mediator.Send(new GetAllFoodCategoriesQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<FoodCategoryResponse>>> GetById(int id)
    {
        var result = await _mediator.Send(new GetFoodCategoryByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [Authorize]
    [HttpGet("with-items")]
    public async Task<ActionResult<BaseResponse<List<FoodCategoryWithItemsResponse>>>> GetCategoriesWithItems(
    [FromQuery] GetFoodCategoriesWithItemsRequest request)
    {
        var result = await _mediator.Send(new GetFoodCategoriesWithItemsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
