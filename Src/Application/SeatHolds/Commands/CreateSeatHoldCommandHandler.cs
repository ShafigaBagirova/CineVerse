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


public sealed class CreateSeatHoldCommandHandler : IRequestHandler<CreateSeatHoldCommand, BaseResponse>
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

    public async Task<BaseResponse> Handle(
        CreateSeatHoldCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;


        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "CreateSeatHoldCommand failed. Authenticated user not found.");

            return BaseResponse.Fail("Authenticated user not found.");
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

            return BaseResponse.Fail("Screening not found.");
        }

        var seat = await _seatRepository.GetByIdAsync(dto.SeatId, cancellationToken);
        if (seat is null)
        {
            _logger.LogWarning(
                "CreateSeatHoldCommand failed. Seat not found. SeatId: {SeatId}",
                dto.SeatId);

            return BaseResponse.Fail("Seat not found.");
        }

        if (seat.HallId != screening.HallId)
        {
            _logger.LogWarning(
                "CreateSeatHoldCommand failed. Seat does not belong to screening hall. SeatId: {SeatId}, SeatHallId: {SeatHallId}, ScreeningHallId: {ScreeningHallId}",
                seat.Id,
                seat.HallId,
                screening.HallId);

            return BaseResponse.Fail("Selected seat does not belong to the screening hall.");
        }

        var activeHold = await _seatHoldRepository.GetActiveHoldAsync(dto.ScreeningId, dto.SeatId, cancellationToken);

        if (activeHold is not null)
        {
            if (activeHold.ExpiresAtUtc > DateTime.UtcNow)
            {
                if (string.Equals(activeHold.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "CreateSeatHold: returning existing active hold for same user. SeatHoldId: {SeatHoldId}",
                        activeHold.Id);

                    return BaseResponse.Ok(
                        "Seat hold already active.");
                }

                _logger.LogWarning(
                    "CreateSeatHoldCommand failed. Active seat hold already exists. SeatHoldId: {SeatHoldId}, ScreeningId: {ScreeningId}, SeatId: {SeatId}, ExpiresAtUtc: {ExpiresAtUtc}",
                    activeHold.Id,
                    dto.ScreeningId,
                    dto.SeatId,
                    activeHold.ExpiresAtUtc);

                return BaseResponse.Fail("This seat is currently on hold.");
            }

            activeHold.Status = SeatHoldStatus.Expired;
            await _seatHoldRepository.UpdateAsync(activeHold, cancellationToken);

            _logger.LogInformation("Expired seat hold marked as expired. SeatHoldId: {SeatHoldId}",
                activeHold.Id);}

        var seatHold = _mapper.Map<SeatHold>(dto);
        seatHold.UserId = userId;
        seatHold.Status = SeatHoldStatus.Active;
        seatHold.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1);

        await _seatHoldRepository.AddAsync(seatHold, cancellationToken);
        await _seatHoldRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seat hold created successfully. SeatHoldId: {SeatHoldId}, ScreeningId: {ScreeningId}, SeatId: {SeatId}, UserId: {UserId}, ExpiresAtUtc: {ExpiresAtUtc}",
            seatHold.Id,
            seatHold.ScreeningId,
            seatHold.SeatId,
            seatHold.UserId,
            seatHold.ExpiresAtUtc);

        await _cacheService.RemoveAsync(SeatHoldCacheKeys.GetAllSeatHolds, cancellationToken);
        await _cacheService.RemoveAsync(
            $"{SeatHoldCacheKeys.GetSeatHoldsByScreeningPrefix}{dto.ScreeningId}",
            cancellationToken);

        _logger.LogInformation(
            "SeatHold cache invalidated after create. GetAllKey: {GetAllKey}, ScreeningKey: {ScreeningKey}",
            SeatHoldCacheKeys.GetAllSeatHolds,
            $"{SeatHoldCacheKeys.GetSeatHoldsByScreeningPrefix}{dto.ScreeningId}");

        return BaseResponse.Ok("Seat hold created successfully.");
    }

}