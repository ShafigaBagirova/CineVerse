using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Payments.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Payments.Queries;

public sealed class GetRefundHistoryQueryHandler
    : IRequestHandler<GetRefundHistoryQuery, BaseResponse<PaginatedResponse<GetRefundHistoryResponse>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetRefundHistoryQueryHandler> _logger;

    public GetRefundHistoryQueryHandler(
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<GetRefundHistoryQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<GetRefundHistoryResponse>>> Handle(
        GetRefundHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;

        _logger.LogInformation(
            "GetRefundHistoryQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}, Provider: {Provider}, UserId: {UserId}",
            dto.PageNumber,
            dto.PageSize,
            dto.Provider,
            dto.UserId);

        var (payments, totalCount) = await _paymentRepository.GetRefundHistoryAsync(
            dto.Provider,
            dto.UserId,
            dto.FromDateUtc,
            dto.ToDateUtc,
            dto.PageNumber,
            dto.PageSize,
            cancellationToken);

        var response = new PaginatedResponse<GetRefundHistoryResponse>
        {
            Items = _mapper.Map<List<GetRefundHistoryResponse>>(payments),
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "GetRefundHistoryQuery completed successfully. TotalCount: {TotalCount}",
            totalCount);

        return BaseResponse<PaginatedResponse<GetRefundHistoryResponse>>.Ok(
            response,
            "Refund history retrieved successfully.");
    }
}