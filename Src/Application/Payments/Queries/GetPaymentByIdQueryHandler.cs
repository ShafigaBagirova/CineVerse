using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Payments.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Payments.Queries;

public sealed class GetPaymentByIdQueryHandler
    : IRequestHandler<GetPaymentByIdQuery, BaseResponse<GetPaymentByIdResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetPaymentByIdQueryHandler(
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<GetPaymentByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetPaymentByIdResponse>> Handle(
        GetPaymentByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetPaymentByIdQuery started. PaymentId: {PaymentId}", request.Id);

        var cacheKey = $"payment:{request.Id}";

        var cached = await _cacheService.GetAsync<GetPaymentByIdResponse>(cacheKey);
        if (cached is not null)
        {
            _logger.LogInformation("Payment returned from cache. PaymentId: {PaymentId}", request.Id);

            return BaseResponse<GetPaymentByIdResponse>.Ok(cached, "Payment retrieved successfully.");
        }

        var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null)
        {
            _logger.LogWarning("Payment not found. PaymentId: {PaymentId}", request.Id);
            return BaseResponse<GetPaymentByIdResponse>.Fail("Payment not found.");
        }

        var response = _mapper.Map<GetPaymentByIdResponse>(payment);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        _logger.LogInformation("GetPaymentByIdQuery completed successfully. PaymentId: {PaymentId}", request.Id);

        return BaseResponse<GetPaymentByIdResponse>.Ok(response, "Payment retrieved successfully.");
    }
}