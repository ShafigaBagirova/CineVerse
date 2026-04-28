using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Common.Helpers;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Seats.Commands;

public sealed class CreateSeatCommandHandler : IRequestHandler<CreateSeatCommand, BaseResponse>
{
    private readonly ISeatRepository _seatRepository;
    private readonly IHallRepository _hallRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSeatCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateSeatCommandHandler(
        ISeatRepository seatRepository,
        IHallRepository hallRepository,
        IMapper mapper,
        ILogger<CreateSeatCommandHandler> logger,
        ICacheService cacheService)
    {
        _seatRepository = seatRepository;
        _hallRepository = hallRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(CreateSeatCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "CreateSeatCommand started. HallId: {HallId}, Row: {Row}, Number: {Number}, Type: {Type}",
            request.Request.HallId,
            request.Request.Row,
            request.Request.Number,
            request.Request.Type);

        var hallExists = await _hallRepository.ExistsAsync(request.Request.HallId, cancellationToken);
        if (!hallExists)
        {
            _logger.LogWarning(
                "CreateSeatCommand failed. Hall not found. HallId: {HallId}",
                request.Request.HallId);

            return BaseResponse.Fail("Hall not found.");
        }

        var normalizedRow = request.Request.Row.Trim().ToUpperInvariant();

        var seatExists = await _seatRepository.ExistsAsync(
            request.Request.HallId,
            normalizedRow,
            request.Request.Number,
            cancellationToken);

        if (seatExists)
        {
            _logger.LogWarning(
                "CreateSeatCommand failed. Seat already exists. HallId: {HallId}, Row: {Row}, Number: {Number}",
                request.Request.HallId,
                normalizedRow,
                request.Request.Number);

            return BaseResponse.Fail("A seat with the same row and number already exists in this hall.");
        }

        var seat = _mapper.Map<Seat>(request.Request);
        seat.Row = normalizedRow;

        await _seatRepository.AddAsync(seat, cancellationToken);
        await _seatRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreateSeatCommand completed successfully. SeatId: {SeatId}, HallId: {HallId}",
            seat.Id,
            seat.HallId);

        await _cacheService.RemoveByPrefixAsync("seats_all_hall_");
        await _cacheService.RemoveAsync($"seat_{seat.Id}");
        await _cacheService.RemoveByPrefixAsync("screening_");
        await _cacheService.RemoveByPrefixAsync(ScreeningSeatCacheKeys.GetOccupiedSeatsByScreeningPrefix);
        await _cacheService.RemoveByPrefixAsync(ScreeningSeatCacheKeys.GetAvailableSeatsByScreeningPrefix);

        _logger.LogInformation(
            "Seat cache invalidated for SeatId: {SeatId}, HallId: {HallId}",
            seat.Id,
            seat.HallId);

        return BaseResponse.Ok("Seat created successfully.");
    }
}
