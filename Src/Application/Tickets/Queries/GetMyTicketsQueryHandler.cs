using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Tickets.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Tickets.Queries;

public sealed class GetMyTicketsQueryHandler
    : IRequestHandler<GetMyTicketsQuery, BaseResponse<PaginatedResponse<GetMyTicketsResponse>>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyTicketsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetMyTicketsQueryHandler(
        ITicketRepository ticketRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetMyTicketsQueryHandler> logger,
        ICacheService cacheService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetMyTicketsResponse>>> Handle(
        GetMyTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetMyTicketsQuery failed. Authenticated user not found.");
            return BaseResponse<PaginatedResponse<GetMyTicketsResponse>>.Fail("Authenticated user not found.");
        }

        var filter = request.Request;

        var cacheKey =
            $"{TicketCacheKey.GetMyTicketsPrefix}{userId}:{JsonSerializer.Serialize(filter)}";

        var cached = await _cacheService.GetAsync<PaginatedResponse<GetMyTicketsResponse>>(
            cacheKey,
            cancellationToken);

        if (cached is not null)
        {
            _logger.LogInformation(
                "GetMyTicketsQuery fetched from cache. UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                userId,
                filter.PageNumber,
                filter.PageSize);

            return BaseResponse<PaginatedResponse<GetMyTicketsResponse>>.Ok(cached);
        }

        _logger.LogInformation(
            "GetMyTicketsQuery started. UserId: {UserId}, Status: {Status}, PurchasedAfterUtc: {PurchasedAfterUtc}, PurchasedBeforeUtc: {PurchasedBeforeUtc}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            userId,
            filter.Status,
            filter.PurchasedAfterUtc,
            filter.PurchasedBeforeUtc,
            filter.PageNumber,
            filter.PageSize);

        var (items, totalCount) = await _ticketRepository.GetMyTicketsAsync(
            userId,
            filter.Status,
            filter.PurchasedAfterUtc,
            filter.PurchasedBeforeUtc,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetMyTicketsResponse>>(items);

        var response = new PaginatedResponse<GetMyTicketsResponse>
        {
            Items = mappedItems,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        _logger.LogInformation(
            "GetMyTicketsQuery completed successfully. UserId: {UserId}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            userId,
            mappedItems.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<GetMyTicketsResponse>>.Ok(response);
    }
}