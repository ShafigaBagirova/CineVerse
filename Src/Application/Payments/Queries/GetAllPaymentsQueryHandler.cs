using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Payments.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Payments.Queries;

public sealed class GetAllPaymentsQueryHandler
    : IRequestHandler<GetAllPaymentsQuery, BaseResponse<PaginatedResponse<GetAllPaymentsResponse>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllPaymentsQueryHandler> _logger;

    public GetAllPaymentsQueryHandler(
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<GetAllPaymentsQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllPaymentsResponse>>> Handle(
        GetAllPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;

        _logger.LogInformation(
            "GetAllPaymentsQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}, Status: {Status}, Provider: {Provider}, UserId: {UserId}",
            dto.PageNumber,
            dto.PageSize,
            dto.Status,
            dto.Provider,
            dto.UserId);

        var (payments, totalCount) = await _paymentRepository.GetPagedAsync(
            dto.Status,
            dto.Provider,
            dto.UserId,
            dto.FromDateUtc,
            dto.ToDateUtc,
            dto.PageNumber,
            dto.PageSize,
            cancellationToken);

        var response = new PaginatedResponse<GetAllPaymentsResponse>
        {
            Items = _mapper.Map<List<GetAllPaymentsResponse>>(payments),
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "GetAllPaymentsQuery completed successfully. TotalCount: {TotalCount}",
            totalCount);

        return BaseResponse<PaginatedResponse<GetAllPaymentsResponse>>.Ok(
            response,
            "Payments retrieved successfully.");
    }
}