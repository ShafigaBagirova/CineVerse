using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.SeatHolds.Queries;

public sealed class GetAllSeatHoldsQueryHandler
    : IRequestHandler<GetAllSeatHoldsQuery, BaseResponse<PaginatedResponse<GetAllSeatHoldsResponse>>>
{
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllSeatHoldsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllSeatHoldsQueryHandler(
        ISeatHoldRepository seatHoldRepository,
        IMapper mapper,
        ILogger<GetAllSeatHoldsQueryHandler> logger,
        ICacheService cacheService)
    {
        _seatHoldRepository = seatHoldRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllSeatHoldsResponse>>> Handle(
        GetAllSeatHoldsQuery request,
        CancellationToken cancellationToken)
    {
        var filter = request.Request;

        var cacheKey =
            $"{SeatHoldCacheKeys.GetAllSeatHoldsPrefix}" +
            $"{JsonSerializer.Serialize(filter)}";

        var cached = await _cacheService.GetAsync<PaginatedResponse<GetAllSeatHoldsResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation(
                "GetAllSeatHoldsQuery fetched from cache. PageNumber: {PageNumber}, PageSize: {PageSize}",
                filter.PageNumber,
                filter.PageSize);

            return BaseResponse<PaginatedResponse<GetAllSeatHoldsResponse>>.Ok(cached);
        }

        _logger.LogInformation(
            "GetAllSeatHoldsQuery started. UserId: {UserId}, Status: {Status}, ExpiresBeforeUtc: {ExpiresBeforeUtc}, ExpiresAfterUtc: {ExpiresAfterUtc}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            filter.UserId,
            filter.Status,
            filter.ExpiresBeforeUtc,
            filter.ExpiresAfterUtc,
            filter.PageNumber,
            filter.PageSize);

        var (items, totalCount) = await _seatHoldRepository.GetPagedAsync(
            filter.UserId,
            filter.Status,
            filter.ExpiresBeforeUtc,
            filter.ExpiresAfterUtc,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetAllSeatHoldsResponse>>(items);

        var response = new PaginatedResponse<GetAllSeatHoldsResponse>
        {
            Items = mappedItems,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        _logger.LogInformation(
            "GetAllSeatHoldsQuery completed successfully. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            mappedItems.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<GetAllSeatHoldsResponse>>.Ok(response);
    }
}
