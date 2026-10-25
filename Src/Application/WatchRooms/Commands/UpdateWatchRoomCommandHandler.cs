using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchRooms.Commands;

public class UpdateWatchRoomCommandHandler : IRequestHandler<UpdateWatchRoomCommand, BaseResponse>
{
    private readonly IWatchRoomRepository _watchRoomRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateWatchRoomCommandHandler> _logger;

    public UpdateWatchRoomCommandHandler(
        IWatchRoomRepository watchRoomRepository,
        ICurrentUserService currentUserService,
        ILogger<UpdateWatchRoomCommandHandler> logger)
    {
        _watchRoomRepository = watchRoomRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(UpdateWatchRoomCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        _logger.LogInformation(
            "UpdateWatchRoomCommand started. WatchRoomId: {WatchRoomId}, UserId: {UserId}",
            request.Id,
            currentUserId);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse.Fail("Authenticated user not found.");

        var room = await _watchRoomRepository.GetByIdAsync(request.Id, cancellationToken);

        if (room is null || !room.IsActive)
        {
            _logger.LogWarning("Watch room not found for update. Id: {WatchRoomId}", request.Id);
            return BaseResponse.Fail("Watch room not found.");
        }

        if (room.HostUserId != currentUserId)
        {
            _logger.LogWarning(
                "UpdateWatchRoomCommand failed. User is not the host. WatchRoomId: {WatchRoomId}, UserId: {UserId}",
                request.Id,
                currentUserId);

            return BaseResponse.Fail("Only the host can update this watch room.");
        }

        if (request.Request.Title is not null)
        {
            var title = request.Request.Title.Trim();
            room.Title = string.IsNullOrWhiteSpace(title) ? null : title;
        }

        await _watchRoomRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UpdateWatchRoomCommand completed successfully. WatchRoomId: {WatchRoomId}",
            room.Id);

        return BaseResponse.Ok("Watch room updated successfully.");
    }
}
