using Application.Common.Responses;
using Application.Notifications.Commands;
using Application.Notifications.Dtos;
using Application.Notifications.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("my")]
    [Authorize(Policy=Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetMyNotificationsResponse>>>> GetMyNotifications(
        [FromQuery] GetMyNotificationsRequest request)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<BaseResponse>> MarkAsRead(int id)
    {
        var result = await _mediator.Send(new MarkNotificationAsReadCommand(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<BaseResponse<GetUnreadNotificationCountResponse>>> GetUnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadNotificationCountQuery());

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("recommendations/refresh")]
    public async Task<ActionResult<BaseResponse>> RefreshRecommendations()
    {
        var result = await _mediator.Send(new NotifyAllUsersRecommendationsCommand());

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
