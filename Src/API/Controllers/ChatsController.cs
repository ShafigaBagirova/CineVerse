using Application.Chats.Commands;
using Application.Chats.Dtos;
using Application.Chats.Queries;
using Application.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/chats")]
[ApiController]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("private/{receiverUserId}")]
    public async Task<ActionResult<BaseResponse<ChatDto>>> CreatePrivateChat([FromRoute] string receiverUserId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreatePrivateChatCommand(receiverUserId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<BaseResponse<List<ChatDto>>>> GetMyChats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyChatsQuery(), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{chatId:int}/messages")]
    public async Task<ActionResult<BaseResponse<List<MessageDto>>>> GetChatMessages([FromRoute] int chatId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChatMessagesQuery(chatId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
