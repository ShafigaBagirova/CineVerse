using Application.Chats.Commands;
using Application.Chats.Dtos;
using Application.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using API.Hubs;

namespace API.Controllers;

[Route("api/messages")]
[ApiController]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessagesController(IMediator mediator, IHubContext<ChatHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<MessageDto>>> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendMessageCommand(request), cancellationToken);
        if (!result.Success || result.Data is null) return BadRequest(result);

        await _hubContext.Clients.Group($"chat:{request.ChatId}").SendAsync("ReceiveMessage", result.Data, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{chatId:int}/read")]
    public async Task<ActionResult<BaseResponse>> MarkMessagesAsRead([FromRoute] int chatId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkMessagesAsReadCommand(chatId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
