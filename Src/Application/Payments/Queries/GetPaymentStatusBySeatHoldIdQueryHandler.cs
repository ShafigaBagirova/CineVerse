using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Payments.Dtos;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Payments.Queries;

public sealed class GetPaymentStatusBySeatHoldIdQueryHandler
    : IRequestHandler<GetPaymentStatusBySeatHoldIdQuery, BaseResponse<GetPaymentStatusBySeatHoldIdResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentStatusBySeatHoldIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetPaymentStatusBySeatHoldIdQueryHandler(
        IPaymentRepository paymentRepository,
        ISeatHoldRepository seatHoldRepository,
        IMapper mapper,
        ILogger<GetPaymentStatusBySeatHoldIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _paymentRepository = paymentRepository;
        _seatHoldRepository = seatHoldRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetPaymentStatusBySeatHoldIdResponse>> Handle(
        GetPaymentStatusBySeatHoldIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetPaymentStatusBySeatHoldIdQuery started. SeatHoldId: {SeatHoldId}",
            request.SeatHoldId);

        var cacheKey = $"payment-status-seatHold:{request.SeatHoldId}";

        var cachedResponse = await _cacheService.GetAsync<GetPaymentStatusBySeatHoldIdResponse>(cacheKey);
        if (cachedResponse is not null)
        {
            _logger.LogInformation(
                "Payment status returned from cache. SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            return BaseResponse<GetPaymentStatusBySeatHoldIdResponse>.Ok(
                cachedResponse,
                "Payment status retrieved successfully.");
        }

        var seatHoldExists = await _seatHoldRepository.ExistsAsync(request.SeatHoldId, cancellationToken);
        if (!seatHoldExists)
        {
            _logger.LogWarning(
                "SeatHold not found for payment status query. SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            return BaseResponse<GetPaymentStatusBySeatHoldIdResponse>.Fail("Seat hold not found.");
        }

        var payment = await _paymentRepository.GetBySeatHoldIdAsync(request.SeatHoldId, cancellationToken);

        GetPaymentStatusBySeatHoldIdResponse response;

        if (payment is null)
        {
            _logger.LogInformation(
                "No payment found for SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            response = new GetPaymentStatusBySeatHoldIdResponse
            {
                SeatHoldId = request.SeatHoldId,
                HasPayment = false,
                Status = PaymentStatus.Pending,
                Amount = null,
                Currency = null,
                PaymentIntentId = null,
                PaidAtUtc = null
            };
        }
        else
        {
            response = _mapper.Map<GetPaymentStatusBySeatHoldIdResponse>(payment);
        }

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromSeconds(20));

        _logger.LogInformation(
            "GetPaymentStatusBySeatHoldIdQuery completed successfully. SeatHoldId: {SeatHoldId}, HasPayment: {HasPayment}, Status: {Status}",
            request.SeatHoldId,
            response.HasPayment,
            response.Status);

        return BaseResponse<GetPaymentStatusBySeatHoldIdResponse>.Ok(
            response,
            "Payment status retrieved successfully.");
    }
}