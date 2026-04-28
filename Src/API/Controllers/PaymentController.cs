using Application.Common.Responses;
using Application.Payments.Commands;
using Application.Payments.Dtos;
using Application.Payments.Queries;
using Domain.Constants;
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
    [Authorize(Policy = Policies.Authenticated)]
    [HttpPost("create-intent")]
    public async Task<ActionResult<BaseResponse<CreatePaymentIntentResponse>>> CreateIntent(
    [FromBody] CreatePaymentIntentRequest request)
    {
        var result = await _mediator.Send(new CreatePaymentIntentCommand(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Policy = Policies.Authenticated)]
    [HttpPost("vip/create-intent")]
    public async Task<ActionResult<BaseResponse<CreateVipPaymentIntentResponse>>> CreateVipIntent(CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateVipPaymentIntentCommand(), ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _mediator.Send(
            new ProcessStripeWebhookCommand(json, signature));

        if (!result.Success)
            return BadRequest(result);

        return Ok();
    }
    [Authorize(Policy = Policies.PurchaseTicket)]
    [HttpGet("seat-hold/{seatHoldId:int}/status")]
    public async Task<ActionResult<BaseResponse<GetPaymentStatusBySeatHoldIdResponse>>> GetPaymentStatusBySeatHoldId(int seatHoldId)
    {
        var result = await _mediator.Send(new GetPaymentStatusBySeatHoldIdQuery(seatHoldId));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.Authenticated)]
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<GetPaymentByIdResponse>>> GetById(int id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.Authenticated)]
    [HttpGet("my")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetMyPaymentsResponse>>>> GetMyPayments(
        [FromQuery] GetMyPaymentsRequest request)
    {
        var result = await _mediator.Send(new GetMyPaymentsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.PurchaseTicket)]
    [HttpPost("retry/{seatHoldId:int}")]
    public async Task<ActionResult<BaseResponse<RetryPaymentResponse>>> RetryPayment(int seatHoldId)
    {
        var result = await _mediator.Send(new RetryPaymentCommand(seatHoldId));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy = Policies.ManageCinemas)]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetAllPaymentsResponse>>>> GetAllPayments(
    [FromQuery] GetAllPaymentsRequest request)
    {
        var result = await _mediator.Send(new GetAllPaymentsQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Policy =Policies.ManageCinemas)]
    [HttpGet("refund-history")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<GetRefundHistoryResponse>>>> GetRefundHistory(
    [FromQuery] GetRefundHistoryRequest request)
    {
        var result = await _mediator.Send(new GetRefundHistoryQuery(request));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
