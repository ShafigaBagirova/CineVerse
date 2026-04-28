using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Screenings.Commands;

public sealed class CreateScreeningCommandHandler : IRequestHandler<CreateScreeningCommand, BaseResponse>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateScreeningCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateScreeningCommandHandler(
        IScreeningRepository screeningRepository,
        IMovieRepository movieRepository,
        IHallRepository hallRepository,
        IMapper mapper,
        ILogger<CreateScreeningCommandHandler> logger,
        ICacheService cacheService)
    {
        _screeningRepository = screeningRepository;
        _movieRepository = movieRepository;
        _hallRepository = hallRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(CreateScreeningCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Request;

        _logger.LogInformation(
            "CreateScreeningCommand started. MovieId: {MovieId}, HallId: {HallId}, StartTime: {StartTime}, EndTime: {EndTime}, Price: {Price}, Format: {Format}",
            dto.MovieId,
            dto.HallId,
            dto.StartTime,
            dto.EndTime,
            dto.Price,
            dto.Format);

        var movieExists = await _movieRepository.ExistsAsync(dto.MovieId, cancellationToken);
        if (!movieExists)
        {
            _logger.LogWarning(
                "CreateScreeningCommand failed. Movie not found. MovieId: {MovieId}",
                dto.MovieId);

            return BaseResponse.Fail("Movie not found.");
        }

        var hallExists = await _hallRepository.ExistsAsync(dto.HallId, cancellationToken);
        if (!hallExists)
        {
            _logger.LogWarning(
                "CreateScreeningCommand failed. Hall not found. HallId: {HallId}",
                dto.HallId);

            return BaseResponse.Fail("Hall not found.");
        }

        var hasConflict = await _screeningRepository.HasTimeConflictAsync(
            dto.HallId,
            dto.StartTime,
            dto.EndTime,
            cancellationToken);

        if (hasConflict)
        {
            _logger.LogWarning(
                "CreateScreeningCommand failed. Time conflict detected. HallId: {HallId}, StartTime: {StartTime}, EndTime: {EndTime}",
                dto.HallId,
                dto.StartTime,
                dto.EndTime);

            return BaseResponse.Fail("There is already another screening scheduled in this hall for the selected time range.");
        }

        var screening = _mapper.Map<Screening>(dto);

        await _screeningRepository.AddAsync(screening, cancellationToken);
        await _screeningRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreateScreeningCommand completed successfully. ScreeningId: {ScreeningId}, HallId: {HallId}, MovieId: {MovieId}",
            screening.Id,
            screening.HallId,
            screening.MovieId);

        await _cacheService.RemoveAsync("screenings_all");
        await _cacheService.RemoveByPrefixAsync("screenings_all_");
        await _cacheService.RemoveByPrefixAsync("screening_");
        await _cacheService.RemoveAsync($"hall_{screening.HallId}_screenings");
        await _cacheService.RemoveAsync($"movie_{screening.MovieId}_screenings");
        _logger.LogInformation(
            "Screening cache prefixes removed after create: {PrefixAll}, {PrefixById}",
            "screenings_all_",
            "screening_");

        _logger.LogInformation(
            "Screening cache invalidated. ScreeningId: {ScreeningId}, HallId: {HallId}, MovieId: {MovieId}",
            screening.Id,
            screening.HallId,
            screening.MovieId);

        return BaseResponse.Ok("Screening created successfully.");
    }
}
