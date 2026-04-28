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

    public GetPaymentStatusBySeatHoldIdQueryHandler(
        IPaymentRepository paymentRepository,
        ISeatHoldRepository seatHoldRepository,
        IMapper mapper,
        ILogger<GetPaymentStatusBySeatHoldIdQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _seatHoldRepository = seatHoldRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<GetPaymentStatusBySeatHoldIdResponse>> Handle(
        GetPaymentStatusBySeatHoldIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetPaymentStatusBySeatHoldIdQuery started. SeatHoldId: {SeatHoldId}",
            request.SeatHoldId);


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

            return BaseResponse<GetPaymentStatusBySeatHoldIdResponse>.Ok(
                new GetPaymentStatusBySeatHoldIdResponse
                {
                    SeatHoldId = request.SeatHoldId,
                    HasPayment = false,
                    Status = null,
                },
                "No payment found"
            );
        }
        
        response = _mapper.Map<GetPaymentStatusBySeatHoldIdResponse>(payment);

        _logger.LogInformation(
      "FINAL payment status response. SeatHoldId: {SeatHoldId}, HasPayment: {HasPayment}, Status: {Status}", response.SeatHoldId,
      response.HasPayment,response.Status);

        return BaseResponse<GetPaymentStatusBySeatHoldIdResponse>.Ok(
            response,
            "Payment status retrieved successfully.");
    }
}