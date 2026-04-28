using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.SeatHolds.Commands;


public sealed class CreateSeatHoldCommandHandler : IRequestHandler<CreateSeatHoldCommand, BaseResponse<GetSeatHoldByIdResponse>>
{
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSeatHoldCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateSeatHoldCommandHandler(
        ISeatHoldRepository seatHoldRepository,
        IScreeningRepository screeningRepository,
        ISeatRepository seatRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateSeatHoldCommandHandler> logger,
        ICacheService cacheService)
    {
        _seatHoldRepository = seatHoldRepository;
        _screeningRepository = screeningRepository;
        _seatRepository = seatRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetSeatHoldByIdResponse>> Handle(
        CreateSeatHoldCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;


        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "CreateSeatHoldCommand failed. Authenticated user not found.");

            return BaseResponse<GetSeatHoldByIdResponse>.Fail("Authenticated user not found.");
        }
        _logger.LogInformation(
            "CreateSeatHoldCommand started. ScreeningId: {ScreeningId}, SeatId: {SeatId}, UserId: {UserId}",
            dto.ScreeningId,
            dto.SeatId,
            userId);

        var screening = await _screeningRepository.GetByIdAsync(dto.ScreeningId, cancellationToken);
        if (screening is null)
        {
            _logger.LogWarning(
                "CreateSeatHoldCommand failed. Screening not found. ScreeningId: {ScreeningId}",
                dto.ScreeningId);

            return BaseResponse<GetSeatHoldByIdResponse>.Fail("Screening not found.");
        }

        var seat = await _seatRepository.GetByIdAsync(dto.SeatId, cancellationToken);
        if (seat is null)
        {
            _logger.LogWarning(
                "CreateSeatHoldCommand failed. Seat not found. SeatId: {SeatId}",
                dto.SeatId);

            return BaseResponse<GetSeatHoldByIdResponse>.Fail("Seat not found.");
        }

        if (seat.HallId != screening.HallId)
        {
            _logger.LogWarning(
                "CreateSeatHoldCommand failed. Seat does not belong to screening hall. SeatId: {SeatId}, SeatHallId: {SeatHallId}, ScreeningHallId: {ScreeningHallId}",
                seat.Id,
                seat.HallId,
                screening.HallId);

            return BaseResponse<GetSeatHoldByIdResponse>.Fail("Selected seat does not belong to the screening hall.");
        }

        var activeHold = await _seatHoldRepository.GetActiveHoldAsync(dto.ScreeningId, dto.SeatId, cancellationToken);

        if (activeHold is not null)
        {
            if (activeHold.ExpiresAtUtc > DateTime.UtcNow)
            {
                if (string.Equals(activeHold.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    var hasBlockingPayment = activeHold.Payments.Any(p =>
                        p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Succeeded);
                    if (hasBlockingPayment)
                    {
                        _logger.LogInformation(
                            "CreateSeatHold: returning existing active hold for same user with blocking payment status. SeatHoldId: {SeatHoldId}",
                            activeHold.Id);

                        var existingResponse = _mapper.Map<GetSeatHoldByIdResponse>(activeHold);
                        return BaseResponse<GetSeatHoldByIdResponse>.Ok(
                            existingResponse,
                            "Seat hold already active.");
                    }

                    // Terminal payment outcomes (Failed/Cancelled/Refunded) should not reuse old hold.
                    activeHold.Status = SeatHoldStatus.Released;
                    await _seatHoldRepository.UpdateAsync(activeHold, cancellationToken);
                    _logger.LogInformation(
                        "CreateSeatHold: existing active hold released due to non-blocking payment status; creating new hold. OldSeatHoldId: {SeatHoldId}",
                        activeHold.Id);
                }
                else
                {
                    _logger.LogInformation(
                        "CreateSeatHoldCommand failed. Active seat hold already exists. SeatHoldId: {SeatHoldId}, ScreeningId: {ScreeningId}, SeatId: {SeatId}, ExpiresAtUtc: {ExpiresAtUtc}",
                        activeHold.Id);
                    return BaseResponse<GetSeatHoldByIdResponse>.Fail("This seat is currently on hold.");
                }
            }

            else
            {
                activeHold.Status = SeatHoldStatus.Expired;
                await _seatHoldRepository.UpdateAsync(activeHold, cancellationToken);

                _logger.LogInformation("Expired seat hold marked as expired. SeatHoldId: {SeatHoldId}",
                    activeHold.Id);
            }
        }

        var seatHold = _mapper.Map<SeatHold>(dto);
        seatHold.UserId = userId;
        seatHold.Status = SeatHoldStatus.Active;
        seatHold.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10);

        await _seatHoldRepository.AddAsync(seatHold, cancellationToken);
        await _seatHoldRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seat hold created successfully. SeatHoldId: {SeatHoldId}, ScreeningId: {ScreeningId}, SeatId: {SeatId}, UserId: {UserId}, ExpiresAtUtc: {ExpiresAtUtc}",
            seatHold.Id,
            seatHold.ScreeningId,
            seatHold.SeatId,
            seatHold.UserId,
            seatHold.ExpiresAtUtc);

        await _cacheService.RemoveByPrefixAsync(SeatHoldCacheKeys.GetAllSeatHoldsPrefix);
        await _cacheService.RemoveByPrefixAsync(
            $"{SeatHoldCacheKeys.GetSeatHoldsByScreeningPrefix}{dto.ScreeningId}");
        await _cacheService.RemoveAsync(
            $"{SeatHoldCacheKeys.GetSeatHoldByIdPrefix}{seatHold.Id}",
            cancellationToken);
        await _cacheService.RemoveAsync(
            $"{ScreeningSeatCacheKeys.GetOccupiedSeatsByScreeningPrefix}{dto.ScreeningId}",
            cancellationToken);
        await _cacheService.RemoveAsync(
            $"{ScreeningSeatCacheKeys.GetAvailableSeatsByScreeningPrefix}{dto.ScreeningId}",
            cancellationToken);

        _logger.LogInformation(
            "SeatHold cache invalidated after create. GetAllKey: {GetAllKey}, ScreeningKey: {ScreeningKey}",
            SeatHoldCacheKeys.GetAllSeatHolds,
            $"{SeatHoldCacheKeys.GetSeatHoldsByScreeningPrefix}{dto.ScreeningId}");

        var response = _mapper.Map<GetSeatHoldByIdResponse>(seatHold);
        return BaseResponse<GetSeatHoldByIdResponse>.Ok(response, "Seat hold created successfully.");
    }

}