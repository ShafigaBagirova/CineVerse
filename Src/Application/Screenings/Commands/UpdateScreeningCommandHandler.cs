using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Screenings.Commands;

public sealed class UpdateScreeningCommandHandler : IRequestHandler<UpdateScreeningCommand, BaseResponse>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateScreeningCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateScreeningCommandHandler(
        IScreeningRepository screeningRepository,
        IMovieRepository movieRepository,
        IHallRepository hallRepository,
        IMapper mapper,
        ILogger<UpdateScreeningCommandHandler> logger,
        ICacheService cacheService)
    {
        _screeningRepository = screeningRepository;
        _movieRepository = movieRepository;
        _hallRepository = hallRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(UpdateScreeningCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Request;

        _logger.LogInformation(
            "UpdateScreeningCommand started. ScreeningId: {ScreeningId}, MovieId: {MovieId}, HallId: {HallId}, StartTime: {StartTime}, EndTime: {EndTime}, Price: {Price}, Format: {Format}, Status: {Status}, IsActive: {IsActive}",
            request.Id,
            dto.MovieId,
            dto.HallId,
            dto.StartTime,
            dto.EndTime,
            dto.Price,
            dto.Format,
            dto.Status,
            dto.IsActive);

        var screening = await _screeningRepository.GetByIdAsync(request.Id, cancellationToken);

        if (screening is null)
        {
            _logger.LogWarning(
                "UpdateScreeningCommand failed. Screening not found. ScreeningId: {ScreeningId}",
                request.Id);

            return BaseResponse.Fail("Screening not found.");
        }

        var oldHallId = screening.HallId;
        var oldMovieId = screening.MovieId;

        var targetMovieId = dto.MovieId ?? screening.MovieId;
        var targetHallId = dto.HallId ?? screening.HallId;
        var targetStartTime = dto.StartTime ?? screening.StartTime;
        var targetEndTime = dto.EndTime ?? screening.EndTime;

        if (dto.MovieId.HasValue)
        {
            var movieExists = await _movieRepository.ExistsAsync(targetMovieId, cancellationToken);
            if (!movieExists)
            {
                _logger.LogWarning(
                    "UpdateScreeningCommand failed. Movie not found. MovieId: {MovieId}",
                    targetMovieId);

                return BaseResponse.Fail("Movie not found.");
            }
        }

        if (dto.HallId.HasValue)
        {
            var hallExists = await _hallRepository.ExistsAsync(targetHallId, cancellationToken);
            if (!hallExists)
            {
                _logger.LogWarning(
                    "UpdateScreeningCommand failed. Hall not found. HallId: {HallId}",
                    targetHallId);

                return BaseResponse.Fail("Hall not found.");
            }
        }

        if (targetEndTime <= targetStartTime)
        {
            _logger.LogWarning(
                "UpdateScreeningCommand failed. Invalid time range. ScreeningId: {ScreeningId}, StartTime: {StartTime}, EndTime: {EndTime}",
                request.Id,
                targetStartTime,
                targetEndTime);

            return BaseResponse.Fail("End time must be greater than start time.");
        }

        var hasConflict = await _screeningRepository.HasTimeConflictAsync(
            targetHallId,
            targetStartTime,
            targetEndTime,
            request.Id,
            cancellationToken);

        if (hasConflict)
        {
            _logger.LogWarning(
                "UpdateScreeningCommand failed. Time conflict detected. ScreeningId: {ScreeningId}, HallId: {HallId}, StartTime: {StartTime}, EndTime: {EndTime}",
                request.Id,
                targetHallId,
                targetStartTime,
                targetEndTime);

            return BaseResponse.Fail("There is already another screening scheduled in this hall for the selected time range.");
        }

        _mapper.Map(dto, screening);

        await _screeningRepository.UpdateAsync(screening, cancellationToken);
        await _screeningRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UpdateScreeningCommand completed successfully. ScreeningId: {ScreeningId}, HallId: {HallId}, MovieId: {MovieId}",
            screening.Id,
            screening.HallId,
            screening.MovieId);

        await _cacheService.RemoveAsync("screenings_all");
        await _cacheService.RemoveAsync($"screening_{screening.Id}");
        await _cacheService.RemoveAsync($"hall_{oldHallId}_screenings");
        await _cacheService.RemoveAsync($"hall_{screening.HallId}_screenings");
        await _cacheService.RemoveAsync($"movie_{oldMovieId}_screenings");
        await _cacheService.RemoveAsync($"movie_{screening.MovieId}_screenings");

        _logger.LogInformation(
            "Screening cache invalidated. ScreeningId: {ScreeningId}, OldHallId: {OldHallId}, NewHallId: {NewHallId}, OldMovieId: {OldMovieId}, NewMovieId: {NewMovieId}",
            screening.Id,
            oldHallId,
            screening.HallId,
            oldMovieId,
            screening.MovieId);

        return BaseResponse.Ok("Screening updated successfully.");
    }
}
