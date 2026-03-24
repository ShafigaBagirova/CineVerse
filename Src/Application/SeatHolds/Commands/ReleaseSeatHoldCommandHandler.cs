using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.SeatHolds.Commands;

public sealed class ReleaseSeatHoldCommandHandler : IRequestHandler<ReleaseSeatHoldCommand, BaseResponse>
{
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReleaseSeatHoldCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public ReleaseSeatHoldCommandHandler(
        ISeatHoldRepository seatHoldRepository,
        ICurrentUserService currentUserService,
        ILogger<ReleaseSeatHoldCommandHandler> logger,
        ICacheService cacheService)
    {
        _seatHoldRepository = seatHoldRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(ReleaseSeatHoldCommand request, CancellationToken cancellationToken)
    {

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
               "ReleaseSeatHoldCommand failed. Authenticated user not found.");

            return BaseResponse.Fail("Authenticated user not found.");
        }

        _logger.LogInformation(
            "ReleaseSeatHoldCommand started. SeatHoldId: {SeatHoldId}, UserId: {UserId}",
            request.Id,
            userId);

        var seatHold = await _seatHoldRepository.GetByIdAsync(request.Id, cancellationToken);
        if (seatHold is null)
        {
            _logger.LogWarning(
                "ReleaseSeatHoldCommand failed. Seat hold not found. SeatHoldId: {SeatHoldId}",
                request.Id);

            return BaseResponse.Fail("Seat hold not found.");
        }

        if (seatHold.UserId != userId)
        {
            _logger.LogWarning(
                "ReleaseSeatHoldCommand failed. User does not own this seat hold. SeatHoldId: {SeatHoldId}, OwnerUserId: {OwnerUserId}, CurrentUserId: {CurrentUserId}",
                seatHold.Id,
                seatHold.UserId,
                userId);

            return BaseResponse.Fail("You are not allowed to release this seat hold.");
        }

        if (seatHold.Status == SeatHoldStatus.Released)
        {
            _logger.LogWarning(
                "ReleaseSeatHoldCommand failed. Seat hold is already released. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse.Fail("Seat hold is already released.");
        }

        if (seatHold.Status == SeatHoldStatus.Expired)
        {
            _logger.LogWarning(
                "ReleaseSeatHoldCommand failed. Seat hold is already expired. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse.Fail("Seat hold is already expired.");
        }

        if (seatHold.Status == SeatHoldStatus.Purchased)
        {
            _logger.LogWarning(
                "ReleaseSeatHoldCommand failed. Seat hold is already completed. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse.Fail("Purchased seat hold cannot be released.");
        }

        if (seatHold.ExpiresAtUtc <= DateTime.UtcNow)
        {
            seatHold.Status = SeatHoldStatus.Expired;

            await _seatHoldRepository.UpdateAsync(seatHold, cancellationToken);
            await _seatHoldRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Seat hold marked as expired during release request. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse.Fail("Seat hold is already expired.");
        }

        seatHold.Status = SeatHoldStatus.Released;

        await _seatHoldRepository.UpdateAsync(seatHold, cancellationToken);
        await _seatHoldRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(SeatHoldCacheKeys.GetAllSeatHolds, cancellationToken);
        await _cacheService.RemoveAsync(
            $"{SeatHoldCacheKeys.GetSeatHoldsByScreeningPrefix}{seatHold.ScreeningId}",
            cancellationToken);

        _logger.LogInformation(
            "Seat hold released successfully. SeatHoldId: {SeatHoldId}, ScreeningId: {ScreeningId}, SeatId: {SeatId}, UserId: {UserId}",
            seatHold.Id,
            seatHold.ScreeningId,
            seatHold.SeatId,
            userId);

        return BaseResponse.Ok("Seat hold released successfully.");
    }
}