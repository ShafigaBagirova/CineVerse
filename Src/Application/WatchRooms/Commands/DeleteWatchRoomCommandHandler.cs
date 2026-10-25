using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchRooms.Commands;

public class DeleteWatchRoomCommandHandler : IRequestHandler<DeleteWatchRoomCommand, BaseResponse>
{
    private readonly IWatchRoomRepository _watchRoomRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWatchRoomNotifier _watchRoomNotifier;
    private readonly ILogger<DeleteWatchRoomCommandHandler> _logger;

    public DeleteWatchRoomCommandHandler(
        IWatchRoomRepository watchRoomRepository,
        ICurrentUserService currentUserService,
        IWatchRoomNotifier watchRoomNotifier,
        ILogger<DeleteWatchRoomCommandHandler> logger)
    {
        _watchRoomRepository = watchRoomRepository;
        _currentUserService = currentUserService;
        _watchRoomNotifier = watchRoomNotifier;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(DeleteWatchRoomCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        _logger.LogInformation(
            "DeleteWatchRoomCommand started. WatchRoomId: {WatchRoomId}, UserId: {UserId}",
            request.Id,
            currentUserId);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse.Fail("Authenticated user not found.");

        var room = await _watchRoomRepository.GetByIdAsync(request.Id, cancellationToken);

        if (room is null)
        {
            _logger.LogWarning(
                "DeleteWatchRoomCommand failed. Watch room not found. WatchRoomId: {WatchRoomId}",
                request.Id);

            return BaseResponse.Fail("Watch room not found.");
        }

        if (!room.IsActive)
        {
            _logger.LogWarning(
                "DeleteWatchRoomCommand failed. Watch room is already inactive. WatchRoomId: {WatchRoomId}",
                request.Id);

            return BaseResponse.Fail("Watch room is already closed.");
        }

        if (room.HostUserId != currentUserId)
        {
            _logger.LogWarning(
                "DeleteWatchRoomCommand failed. User is not the host. WatchRoomId: {WatchRoomId}, UserId: {UserId}",
                request.Id,
                currentUserId);

            return BaseResponse.Fail("Only the host can delete this watch room.");
        }

        room.IsActive = false;

        await _watchRoomRepository.SaveChangesAsync(cancellationToken);

        await _watchRoomNotifier.RoomClosedAsync(room.Code, cancellationToken);

        _logger.LogInformation(
            "DeleteWatchRoomCommand completed successfully. WatchRoomId: {WatchRoomId}",
            request.Id);

        return BaseResponse.Ok("Watch room deleted successfully.");
    }
}
