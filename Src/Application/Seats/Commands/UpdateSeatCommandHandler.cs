using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Common.Helpers;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Seats.Commands;

public sealed class UpdateSeatCommandHandler : IRequestHandler<UpdateSeatCommand, BaseResponse>
{
    private readonly ISeatRepository _seatRepository;
    private readonly IHallRepository _hallRepository;
    private readonly ILogger<UpdateSeatCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public UpdateSeatCommandHandler(
        ISeatRepository seatRepository,
        IHallRepository hallRepository,
        ILogger<UpdateSeatCommandHandler> logger,
        ICacheService cacheService,IMapper mapper)
    {
        _seatRepository = seatRepository;
        _hallRepository = hallRepository;
        _logger = logger;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<BaseResponse> Handle(UpdateSeatCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateSeatCommand started. SeatId: {SeatId}", request.Id);

        var seat = await _seatRepository.GetByIdAsync(request.Id, cancellationToken);

        if (seat is null)
        {
            _logger.LogWarning("UpdateSeatCommand failed. Seat not found. SeatId: {SeatId}", request.Id);
            return BaseResponse.Fail("Seat not found.");
        }

        var targetHallId = request.Request.HallId ?? seat.HallId;
        var targetRow = request.Request.Row != null
            ? request.Request.Row.Trim().ToUpperInvariant()
            : seat.Row;
        var targetNumber = request.Request.Number ?? seat.Number;

        if (request.Request.HallId.HasValue)
        {
            var hallExists = await _hallRepository.ExistsAsync(targetHallId, cancellationToken);
            if (!hallExists)
            {
                _logger.LogWarning("UpdateSeatCommand failed. Hall not found. HallId: {HallId}", targetHallId);
                return BaseResponse.Fail("Hall not found.");
            }
        }

        var seatExists = await _seatRepository.ExistsAsync(
            targetHallId,
            targetRow,
            targetNumber,
            request.Id,
            cancellationToken);

        if (seatExists)
        {
            _logger.LogWarning(
                "UpdateSeatCommand failed. Duplicate seat exists. HallId: {HallId}, Row: {Row}, Number: {Number}",
                targetHallId,
                targetRow,
                targetNumber);

            return BaseResponse.Fail("A seat with the same row and number already exists in this hall.");
        }

        _mapper.Map(request.Request, seat);

        await _seatRepository.UpdateAsync(seat, cancellationToken);
        await _seatRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("seats_all_hall_");
        await _cacheService.RemoveAsync($"seat_{seat.Id}");
        await _cacheService.RemoveByPrefixAsync("screening_");
        await _cacheService.RemoveByPrefixAsync(ScreeningSeatCacheKeys.GetOccupiedSeatsByScreeningPrefix);
        await _cacheService.RemoveByPrefixAsync(ScreeningSeatCacheKeys.GetAvailableSeatsByScreeningPrefix);

        _logger.LogInformation("UpdateSeatCommand completed successfully. SeatId: {SeatId}", seat.Id);

        return BaseResponse.Ok("Seat updated successfully.");
    }
}