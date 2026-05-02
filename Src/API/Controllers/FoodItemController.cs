using Application.Common.Responses;
using Application.FoodItems.Commands;
using Application.FoodItems.Dtos;
using Application.FoodItems.Queries;
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
    private const string DefaultBucket = "posters";

    public FoodItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private static string? ToApiFileUrl(string? imageValue)
    {
        if (string.IsNullOrWhiteSpace(imageValue))
            return null;

        if (imageValue.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return imageValue;

        var objectName = imageValue.Trim().TrimStart('/');
        var bucketPrefix = $"{DefaultBucket}/";
        if (objectName.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
            objectName = objectName[bucketPrefix.Length..];

        return $"/api/files/{DefaultBucket}/{objectName}";
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create([FromForm] CreateFoodItemRequest request)
    {
        var result = await _mediator.Send(new CreateFoodItemCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Update(int id, [FromBody] UpdateFoodItemRequest request)
    {
        var result = await _mediator.Send(new UpdateFoodItemCommand(id, request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPut("{id:int}/image")]
    public async Task<ActionResult<BaseResponse>> UpdateImage(
    int id,
    [FromForm] UpdateFoodItemImageRequest request,
    CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateFoodItemImageCommand(id, request), ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteFoodItemCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<FoodItemResponse>>>> GetAll(
    [FromQuery] GetAllFoodItemsRequest request)
    {
        var result = await _mediator.Send(new GetAllFoodItemsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        if (result.Data is not null)
        {
            foreach (var item in result.Data)
            {
                item.Image = ToApiFileUrl(item.Image);
            }
        }

        return Ok(result);
    }
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<FoodItemResponse>>> GetById(int id)
    {
        var result = await _mediator.Send(new GetFoodItemByIdQuery(id));

        if (!result.Success)
            return NotFound(result);

        if (result.Data is not null)
        {
            result.Data.Image = ToApiFileUrl(result.Data.Image);
        }

        return Ok(result);
    }
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpGet("top-selling")]
    public async Task<ActionResult<BaseResponse<List<TopSellingFoodItemResponse>>>> GetTopSelling(
    [FromQuery] int take = 5)
    {
        var result = await _mediator.Send(new GetTopSellingFoodItemsQuery(take));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

}
