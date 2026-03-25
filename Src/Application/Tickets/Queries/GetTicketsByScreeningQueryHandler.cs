using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Tickets.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Tickets.Queries;

public sealed class GetTicketsByScreeningQueryHandler
    : IRequestHandler<GetTicketsByScreeningQuery, BaseResponse<PaginatedResponse<GetTicketsByScreeningResponse>>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTicketsByScreeningQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetTicketsByScreeningQueryHandler(
        ITicketRepository ticketRepository,
        IScreeningRepository screeningRepository,
        IMapper mapper,
        ILogger<GetTicketsByScreeningQueryHandler> logger,
        ICacheService cacheService)
    {
        _ticketRepository = ticketRepository;
        _screeningRepository = screeningRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetTicketsByScreeningResponse>>> Handle(
        GetTicketsByScreeningQuery request,
        CancellationToken cancellationToken)
    {
        var filter = request.Request;

        var cacheKey =
            $"{TicketCacheKey.GetTicketsByScreeningPrefix}{request.ScreeningId}:{JsonSerializer.Serialize(filter)}";

        var cached = await _cacheService.GetAsync<PaginatedResponse<GetTicketsByScreeningResponse>>(
            cacheKey,
            cancellationToken);

        if (cached is not null)
        {
            _logger.LogInformation(
                "GetTicketsByScreeningQuery fetched from cache. ScreeningId: {ScreeningId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                request.ScreeningId,
                filter.PageNumber,
                filter.PageSize);

            return BaseResponse<PaginatedResponse<GetTicketsByScreeningResponse>>.Ok(cached);
        }

        _logger.LogInformation(
            "GetTicketsByScreeningQuery started. ScreeningId: {ScreeningId}, Status: {Status}, PurchasedAfterUtc: {PurchasedAfterUtc}, PurchasedBeforeUtc: {PurchasedBeforeUtc}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.ScreeningId,
            filter.Status,
            filter.PurchasedAfterUtc,
            filter.PurchasedBeforeUtc,
            filter.PageNumber,
            filter.PageSize);

        var screening = await _screeningRepository.GetByIdAsync(request.ScreeningId, cancellationToken);
        if (screening is null)
        {
            _logger.LogWarning(
                "GetTicketsByScreeningQuery failed. Screening not found. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<PaginatedResponse<GetTicketsByScreeningResponse>>.Fail("Screening not found.");
        }

        var (items, totalCount) = await _ticketRepository.GetByScreeningAsync(
            request.ScreeningId,
            filter.Status,
            filter.PurchasedAfterUtc,
            filter.PurchasedBeforeUtc,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetTicketsByScreeningResponse>>(items);

        var response = new PaginatedResponse<GetTicketsByScreeningResponse>
        {
            Items = mappedItems,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        _logger.LogInformation(
            "GetTicketsByScreeningQuery completed successfully. ScreeningId: {ScreeningId}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            request.ScreeningId,
            mappedItems.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<GetTicketsByScreeningResponse>>.Ok(response);
    }
}