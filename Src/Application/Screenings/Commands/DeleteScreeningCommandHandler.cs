using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Screenings.Events;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Screenings.Commands;

public sealed class DeleteScreeningCommandHandler : IRequestHandler<DeleteScreeningCommand, BaseResponse>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly ILogger<DeleteScreeningCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IPublisher _publisher; 

    public DeleteScreeningCommandHandler(
        IScreeningRepository screeningRepository,
        ILogger<DeleteScreeningCommandHandler> logger,
        ICacheService cacheService,
        IPublisher publisher) 
    {
        _screeningRepository = screeningRepository;
        _logger = logger;
        _cacheService = cacheService;
        _publisher = publisher;
    }

    public async Task<BaseResponse> Handle(DeleteScreeningCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteScreeningCommand started. ScreeningId: {ScreeningId}",
            request.Id);

        var screening = await _screeningRepository.GetByIdAsync(request.Id, cancellationToken);

        if (screening is null)
        {
            return BaseResponse.Fail("Screening not found.");
        }

        if (!screening.IsActive)
        {
            return BaseResponse.Fail("Screening is already inactive.");
        }

        screening.IsActive = false;
        screening.Status = ScreeningStatus.Cancelled;

        await _screeningRepository.UpdateAsync(screening, cancellationToken);
        await _screeningRepository.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ScreeningCancelledEvent(screening.Id), cancellationToken);

        await _cacheService.RemoveAsync("screenings_all");
        await _cacheService.RemoveByPrefixAsync("screenings_all_");
        await _cacheService.RemoveAsync($"screening_{screening.Id}");
        await _cacheService.RemoveByPrefixAsync("screening_");
        await _cacheService.RemoveAsync($"hall_{screening.HallId}_screenings");
        await _cacheService.RemoveAsync($"movie_{screening.MovieId}_screenings");
        _logger.LogInformation(
            "Screening cache prefixes removed after delete: {PrefixAll}, {PrefixById}",
            "screenings_all_",
            "screening_");

        return BaseResponse.Ok("Screening deleted successfully.");
    }
}