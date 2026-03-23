using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Halls.Commands;

public class DeleteHallCommandHandler : IRequestHandler<DeleteHallCommand, BaseResponse>
{
    private readonly IHallRepository _hallRepository;
    private readonly ILogger<DeleteHallCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteHallCommandHandler(
        IHallRepository hallRepository,
        ILogger<DeleteHallCommandHandler> logger,
        ICacheService cacheService)
    {
        _hallRepository = hallRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(DeleteHallCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteHallCommand started. HallId: {HallId}",
            request.Id);

        var hall = await _hallRepository.GetByIdAsync(request.Id, cancellationToken);

        if (hall is null)
        {
            _logger.LogWarning(
                "DeleteHallCommand failed. Hall not found. HallId: {HallId}",
                request.Id);

            return BaseResponse.Fail("Hall not found.");
        }

        if (!hall.IsActive)
        {
            _logger.LogWarning(
                "DeleteHallCommand failed. Hall is already inactive. HallId: {HallId}",
                request.Id);

            return BaseResponse.Fail("Hall is already inactive.");
        }

        var cinemaId = hall.CinemaId;

        hall.IsActive = false;

        await _hallRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(HallCacheKey.ById(request.Id));
        await _cacheService.RemoveAsync(HallCacheKey.All);
        await _cacheService.RemoveAsync(HallCacheKey.ByCinemaId(cinemaId));

        _logger.LogInformation(
            "DeleteHallCommand completed successfully. HallId: {HallId}",
            request.Id);

        return BaseResponse.Ok("Hall deleted successfully.");
    }
}