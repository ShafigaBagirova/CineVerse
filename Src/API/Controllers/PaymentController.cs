using Application.Common.Responses;
using Application.Payments.Commands;
using Application.Payments.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize]
    [HttpPost("create-intent")]
    public async Task<ActionResult<BaseResponse<CreatePaymentIntentResponse>>> CreateIntent(
    [FromBody] CreatePaymentIntentRequest request)
    {
        var result = await _mediator.Send(new CreatePaymentIntentCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
