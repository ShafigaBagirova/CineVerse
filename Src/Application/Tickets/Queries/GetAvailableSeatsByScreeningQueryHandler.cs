using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Tickets.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Tickets.Queries;

public sealed class GetAvailableSeatsByScreeningQueryHandler
    : IRequestHandler<GetAvailableSeatsByScreeningQuery, BaseResponse<List<GetAvailableSeatsByScreeningResponse>>>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<GetAvailableSeatsByScreeningQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAvailableSeatsByScreeningQueryHandler(
        IScreeningRepository screeningRepository,
        ISeatRepository seatRepository,
        ISeatHoldRepository seatHoldRepository,
        ITicketRepository ticketRepository,
        ILogger<GetAvailableSeatsByScreeningQueryHandler> logger,
        ICacheService cacheService)
    {
        _screeningRepository = screeningRepository;
        _seatRepository = seatRepository;
        _seatHoldRepository = seatHoldRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<GetAvailableSeatsByScreeningResponse>>> Handle(
        GetAvailableSeatsByScreeningQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{ScreeningSeatCacheKeys.GetAvailableSeatsByScreeningPrefix}{request.ScreeningId}";

        var cached = await _cacheService.GetAsync<List<GetAvailableSeatsByScreeningResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation(
                "GetAvailableSeatsByScreeningQuery fetched from cache. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<List<GetAvailableSeatsByScreeningResponse>>.Ok(cached);
        }

        _logger.LogInformation(
            "GetAvailableSeatsByScreeningQuery started. ScreeningId: {ScreeningId}",
            request.ScreeningId);

        var screening = await _screeningRepository.GetByIdAsync(request.ScreeningId, cancellationToken);
        if (screening is null)
        {
            _logger.LogWarning(
                "GetAvailableSeatsByScreeningQuery failed. Screening not found. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<List<GetAvailableSeatsByScreeningResponse>>.Fail("Screening not found.");
        }

        var hallSeats = await _seatRepository.GetByHallIdAsync(screening.HallId, cancellationToken);
        var soldSeatIds = await _ticketRepository.GetSoldSeatIdsByScreeningAsync(request.ScreeningId, cancellationToken);
        var heldSeatIds = await _seatHoldRepository.GetActiveHeldSeatIdsByScreeningAsync(request.ScreeningId, cancellationToken);

        var unavailableSeatIds = soldSeatIds
            .Concat(heldSeatIds)
            .Distinct()
            .ToHashSet();

        var response = hallSeats
            .Where(x => !unavailableSeatIds.Contains(x.Id))
            .Select(x => new GetAvailableSeatsByScreeningResponse
            {
                SeatId = x.Id,
                Row = x.Row,
                Number = x.Number
            })
            .OrderBy(x => x.Row)
            .ThenBy(x => x.Number)
            .ToList();

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(1), cancellationToken);

        _logger.LogInformation(
            "GetAvailableSeatsByScreeningQuery completed successfully. ScreeningId: {ScreeningId}, Count: {Count}",
            request.ScreeningId,
            response.Count);

        return BaseResponse<List<GetAvailableSeatsByScreeningResponse>>.Ok(response);
    }
}