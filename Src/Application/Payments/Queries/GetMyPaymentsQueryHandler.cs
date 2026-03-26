using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Payments.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Payments.Queries;

public sealed class GetMyPaymentsQueryHandler
    : IRequestHandler<GetMyPaymentsQuery, BaseResponse<PaginatedResponse<GetMyPaymentsResponse>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyPaymentsQueryHandler> _logger;

    public GetMyPaymentsQueryHandler(
        IPaymentRepository paymentRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetMyPaymentsQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<GetMyPaymentsResponse>>> Handle(
        GetMyPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse<PaginatedResponse<GetMyPaymentsResponse>>.Fail("User is not authenticated.");

        _logger.LogInformation(
            "GetMyPaymentsQuery started. UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}, Status: {Status}",
            userId,
            request.Request.PageNumber,
            request.Request.PageSize,
            request.Request.Status);

        var (payments, totalCount) = await _paymentRepository.GetByUserIdAsync(
            userId,
            request.Request.Status,
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);

        var response = new PaginatedResponse<GetMyPaymentsResponse>
        {
            Items = _mapper.Map<List<GetMyPaymentsResponse>>(payments),
            PageNumber = request.Request.PageNumber,
            PageSize = request.Request.PageSize,
            TotalCount = totalCount
        };

        return BaseResponse<PaginatedResponse<GetMyPaymentsResponse>>.Ok(
            response,
            "Payments retrieved successfully.");
    }
}