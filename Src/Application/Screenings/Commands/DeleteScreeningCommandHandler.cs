using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Screenings.Commands;

public sealed class DeleteScreeningCommandHandler : IRequestHandler<DeleteScreeningCommand, BaseResponse>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly ILogger<DeleteScreeningCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteScreeningCommandHandler(
        IScreeningRepository screeningRepository,
        ILogger<DeleteScreeningCommandHandler> logger,
        ICacheService cacheService)
    {
        _screeningRepository = screeningRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(DeleteScreeningCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteScreeningCommand started. ScreeningId: {ScreeningId}",
            request.Id);

        var screening = await _screeningRepository.GetByIdAsync(request.Id, cancellationToken);

        if (screening is null)
        {
            _logger.LogWarning(
                "DeleteScreeningCommand failed. Screening not found. ScreeningId: {ScreeningId}",
                request.Id);

            return BaseResponse.Fail("Screening not found.");
        }

        if (!screening.IsActive)
        {
            _logger.LogWarning(
                "DeleteScreeningCommand failed. Screening is already inactive. ScreeningId: {ScreeningId}",
                request.Id);

            return BaseResponse.Fail("Screening is already inactive.");
        }

        screening.IsActive = false;
        screening.Status = ScreeningStatus.Cancelled;

        await _screeningRepository.UpdateAsync(screening, cancellationToken);
        await _screeningRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DeleteScreeningCommand completed successfully. ScreeningId: {ScreeningId}, HallId: {HallId}, MovieId: {MovieId}",
            screening.Id,
            screening.HallId,
            screening.MovieId);

        await _cacheService.RemoveAsync("screenings_all");
        await _cacheService.RemoveAsync($"screening_{screening.Id}");
        await _cacheService.RemoveAsync($"hall_{screening.HallId}_screenings");
        await _cacheService.RemoveAsync($"movie_{screening.MovieId}_screenings");

        _logger.LogInformation(
            "Screening cache invalidated. ScreeningId: {ScreeningId}, HallId: {HallId}, MovieId: {MovieId}",
            screening.Id,
            screening.HallId,
            screening.MovieId);

        return BaseResponse.Ok("Screening deleted successfully.");
    }
}
