using Application.Common.Interfaces;
using Application.Common.Options;
using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Payments.Commands;

public sealed class CreateVipPaymentIntentCommandHandler(
    ICurrentUserService currentUserService,
    IStripeService stripeService,
    IOptions<VipPaymentOptions> vipOptions,
    ILogger<CreateVipPaymentIntentCommandHandler> logger)
    : IRequestHandler<CreateVipPaymentIntentCommand, BaseResponse<CreateVipPaymentIntentResponse>>
{
    public async Task<BaseResponse<CreateVipPaymentIntentResponse>> Handle(
        CreateVipPaymentIntentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse<CreateVipPaymentIntentResponse>.Fail("Authenticated user not found.");

        var price = vipOptions.Value.MonthlyPriceAzn;
        if (price <= 0)
            return BaseResponse<CreateVipPaymentIntentResponse>.Fail("VIP price is not configured.");

        var idempotencyKey = $"vip-{userId}-{Guid.NewGuid():N}";
        logger.LogInformation("CreateVipPaymentIntent for UserId: {UserId}, Amount: {Amount}", userId, price);

        var stripeResult = await stripeService.CreateVipPaymentIntentAsync(
            price,
            "azn",
            userId,
            idempotencyKey,
            cancellationToken);

        var body = new CreateVipPaymentIntentResponse
        {
            ClientSecret = stripeResult.ClientSecret,
            ProviderPaymentIntentId = stripeResult.PaymentIntentId,
            Amount = price,
            Currency = "azn",
        };

        return BaseResponse<CreateVipPaymentIntentResponse>.Ok(body, "VIP payment intent created.");
    }
}
