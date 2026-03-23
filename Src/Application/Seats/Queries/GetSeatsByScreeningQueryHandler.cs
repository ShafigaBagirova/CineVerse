using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Seats.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Seats.Queries;

public sealed class GetSeatsByScreeningQueryHandler
    : IRequestHandler<GetSeatsByScreeningQuery, BaseResponse<List<GetSeatsByScreeningResponse>>>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ILogger<GetSeatsByScreeningQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetSeatsByScreeningQueryHandler(
        IScreeningRepository screeningRepository,
        ISeatRepository seatRepository,
        ILogger<GetSeatsByScreeningQueryHandler> logger,
        ICacheService cacheService)
    {
        _screeningRepository = screeningRepository;
        _seatRepository = seatRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<GetSeatsByScreeningResponse>>> Handle(
        GetSeatsByScreeningQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"screening_{request.ScreeningId}_seats";

        _logger.LogInformation(
            "GetSeatsByScreeningQuery started. ScreeningId: {ScreeningId}",
            request.ScreeningId);

        var cached =
            await _cacheService.GetAsync<List<GetSeatsByScreeningResponse>>(cacheKey);

        if (cached is not null)
        {
            _logger.LogInformation(
                "GetSeatsByScreeningQuery served from cache. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<List<GetSeatsByScreeningResponse>>
                .Ok(cached, "Seats fetched from cache.");
        }

        var screening = await _screeningRepository.GetByIdAsync(request.ScreeningId, cancellationToken);

        if (screening is null || !screening.IsActive)
        {
            _logger.LogWarning(
                "GetSeatsByScreeningQuery failed. Screening not found. ScreeningId: {ScreeningId}",
                request.ScreeningId);

            return BaseResponse<List<GetSeatsByScreeningResponse>>
                .Fail("Screening not found.");
        }

        var seats = await _seatRepository.GetActiveByHallIdAsync(screening.HallId, cancellationToken);

        var response = seats
            .Select(seat => new GetSeatsByScreeningResponse
            {
                SeatId = seat.Id,
                HallId = seat.HallId,
                Row = seat.Row,
                Number = seat.Number,
                Type = seat.Type,
                IsActive = seat.IsActive,
                Status = SeatAvailabilityStatus.Available
            })
            .OrderBy(x => x.Row)
            .ThenBy(x => x.Number)
            .ToList();

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "GetSeatsByScreeningQuery completed successfully. ScreeningId: {ScreeningId}, SeatCount: {SeatCount}",
            request.ScreeningId,
            response.Count);

        return BaseResponse<List<GetSeatsByScreeningResponse>>
            .Ok(response, "Seats fetched successfully.");
    }
}
