using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Tickets.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Tickets.Queries;

public sealed class GetOccupiedSeatsByScreeningQueryHandler
    : IRequestHandler<GetOccupiedSeatsByScreeningQuery, BaseResponse<List<GetOccupiedSeatsByScreeningResponse>>>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<GetOccupiedSeatsByScreeningQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetOccupiedSeatsByScreeningQueryHandler(
        IScreeningRepository screeningRepository,
        ISeatRepository seatRepository,
        ISeatHoldRepository seatHoldRepository,
        ITicketRepository ticketRepository,
        ILogger<GetOccupiedSeatsByScreeningQueryHandler> logger,
        ICacheService cacheService)
    {
        _screeningRepository = screeningRepository;
        _seatRepository = seatRepository;
        _seatHoldRepository = seatHoldRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<GetOccupiedSeatsByScreeningResponse>>> Handle(
        GetOccupiedSeatsByScreeningQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{ScreeningSeatCacheKeys.GetOccupiedSeatsByScreeningPrefix}{request.ScreeningId}";

        var cached = await _cacheService.GetAsync<List<GetOccupiedSeatsByScreeningResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation(
                "GetOccupiedSeatsByScreeningQuery fetched from cache. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<List<GetOccupiedSeatsByScreeningResponse>>.Ok(cached);
        }

        _logger.LogInformation(
            "GetOccupiedSeatsByScreeningQuery started. ScreeningId: {ScreeningId}",
            request.ScreeningId);

        var screening = await _screeningRepository.GetByIdAsync(request.ScreeningId, cancellationToken);
        if (screening is null)
        {
            _logger.LogWarning(
                "GetOccupiedSeatsByScreeningQuery failed. Screening not found. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<List<GetOccupiedSeatsByScreeningResponse>>.Fail("Screening not found.");
        }

        var hallSeats = await _seatRepository.GetByHallIdAsync(screening.HallId, cancellationToken);
        var soldSeatIds = await _ticketRepository.GetSoldSeatIdsByScreeningAsync(request.ScreeningId, cancellationToken);
        var heldSeatIds = await _seatHoldRepository.GetActiveHeldSeatIdsByScreeningAsync(request.ScreeningId, cancellationToken);

        var soldSeatIdSet = soldSeatIds.ToHashSet();
        var heldSeatIdSet = heldSeatIds.ToHashSet();

        var response = hallSeats
            .Where(x => soldSeatIdSet.Contains(x.Id) || heldSeatIdSet.Contains(x.Id))
            .Select(x => new GetOccupiedSeatsByScreeningResponse
            {
                SeatId = x.Id,
                Row = x.Row,
                Number = x.Number,
                OccupancyType = soldSeatIdSet.Contains(x.Id) ? "Sold" : "Held"
            })
            .OrderBy(x => x.Row)
            .ThenBy(x => x.Number)
            .ToList();

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(1), cancellationToken);

        _logger.LogInformation(
            "GetOccupiedSeatsByScreeningQuery completed successfully. ScreeningId: {ScreeningId}, Count: {Count}",
            request.ScreeningId,
            response.Count);

        return BaseResponse<List<GetOccupiedSeatsByScreeningResponse>>.Ok(response);
    }
}