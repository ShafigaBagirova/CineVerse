using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.SeatHolds.Queries;

public sealed class GetSeatHoldsByScreeningQueryHandler
    : IRequestHandler<GetSeatHoldsByScreeningQuery, BaseResponse<PaginatedResponse<GetSeatHoldsByScreeningResponse>>>
{
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSeatHoldsByScreeningQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetSeatHoldsByScreeningQueryHandler(
        ISeatHoldRepository seatHoldRepository,
        IScreeningRepository screeningRepository,
        IMapper mapper,
        ILogger<GetSeatHoldsByScreeningQueryHandler> logger,
        ICacheService cacheService)
    {
        _seatHoldRepository = seatHoldRepository;
        _screeningRepository = screeningRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetSeatHoldsByScreeningResponse>>> Handle(
        GetSeatHoldsByScreeningQuery request,
        CancellationToken cancellationToken)
    {
        var filter = request.Request;

        var cacheKey =
            $"{SeatHoldCacheKeys.GetSeatHoldsByScreeningPrefix}{request.ScreeningId}:" +
            $"{JsonSerializer.Serialize(filter)}";

        var cached = await _cacheService.GetAsync<PaginatedResponse<GetSeatHoldsByScreeningResponse>>(
            cacheKey,
            cancellationToken);

        if (cached is not null)
        {
            _logger.LogInformation(
                "GetSeatHoldsByScreeningQuery fetched from cache. ScreeningId: {ScreeningId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                request.ScreeningId,
                filter.PageNumber,
                filter.PageSize);

            return BaseResponse<PaginatedResponse<GetSeatHoldsByScreeningResponse>>.Ok(cached);
        }

        _logger.LogInformation(
            "GetSeatHoldsByScreeningQuery started. ScreeningId: {ScreeningId}, Status: {Status}, ExpiresBeforeUtc: {ExpiresBeforeUtc}, ExpiresAfterUtc: {ExpiresAfterUtc}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.ScreeningId,
            filter.Status,
            filter.ExpiresBeforeUtc,
            filter.ExpiresAfterUtc,
            filter.PageNumber,
            filter.PageSize);

        var screeningExists = await _screeningRepository.ExistsAsync(request.ScreeningId, cancellationToken);
        if (!screeningExists)
        {
            _logger.LogWarning(
                "GetSeatHoldsByScreeningQuery failed. Screening not found. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<PaginatedResponse<GetSeatHoldsByScreeningResponse>>.Fail("Screening not found.");
        }

        var (items, totalCount) = await _seatHoldRepository.GetByScreeningAsync(
            request.ScreeningId,
            filter.Status,
            filter.ExpiresBeforeUtc,
            filter.ExpiresAfterUtc,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetSeatHoldsByScreeningResponse>>(items);

        var response = new PaginatedResponse<GetSeatHoldsByScreeningResponse>
        {
            Items = mappedItems,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        _logger.LogInformation(
            "GetSeatHoldsByScreeningQuery completed successfully. ScreeningId: {ScreeningId}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            request.ScreeningId,
            mappedItems.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<GetSeatHoldsByScreeningResponse>>.Ok(response);
    }
}