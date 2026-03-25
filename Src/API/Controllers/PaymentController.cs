using Application.Common.Responses;
using Application.Payments.Commands;
using Application.Payments.Dtos;
using Infrastructure.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        Console.WriteLine("WEBHOOK ACTION HIT");
        using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _mediator.Send(
            new ProcessStripeWebhookCommand(json, signature));

        if (!result.Success)
            return BadRequest(result);

        return Ok();
    }
}
