using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Seats.Commands;

public sealed class DeleteSeatCommandHandler : IRequestHandler<DeleteSeatCommand, BaseResponse>
{
    private readonly ISeatRepository _seatRepository;
    private readonly ILogger<DeleteSeatCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteSeatCommandHandler(
        ISeatRepository seatRepository,
        ILogger<DeleteSeatCommandHandler> logger,
        ICacheService cacheService)
    {
        _seatRepository = seatRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(DeleteSeatCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteSeatCommand started. SeatId: {SeatId}",
            request.Id);

        var seat = await _seatRepository.GetByIdAsync(request.Id, cancellationToken);

        if (seat is null)
        {
            _logger.LogWarning(
                "DeleteSeatCommand failed. Seat not found. SeatId: {SeatId}",
                request.Id);

            return BaseResponse.Fail("Seat not found.");
        }

        if (!seat.IsActive)
        {
            _logger.LogWarning(
                "DeleteSeatCommand failed. Seat is already inactive. SeatId: {SeatId}",
                request.Id);

            return BaseResponse.Fail("Seat is already inactive.");
        }

        seat.IsActive = false;

        await _seatRepository.UpdateAsync(seat, cancellationToken);
        await _seatRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DeleteSeatCommand completed successfully. SeatId: {SeatId}, HallId: {HallId}",
            seat.Id,
            seat.HallId);

        await _cacheService.RemoveAsync("seats_all");
        await _cacheService.RemoveAsync($"seat_{seat.Id}");
        await _cacheService.RemoveAsync($"hall_{seat.HallId}_seats");

        _logger.LogInformation(
            "Seat cache invalidated. SeatId: {SeatId}, HallId: {HallId}",
            seat.Id,
            seat.HallId);

        return BaseResponse.Ok("Seat deleted successfully.");
    }
}