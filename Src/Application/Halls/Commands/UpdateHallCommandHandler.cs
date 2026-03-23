using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Halls.Commands;

public class UpdateHallCommandHandler : IRequestHandler<UpdateHallCommand, BaseResponse>
{
    private readonly IHallRepository _hallRepository;
    private readonly ICinemaRepository _cinemaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateHallCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateHallCommandHandler(
        IHallRepository hallRepository,
        ICinemaRepository cinemaRepository,
        IMapper mapper,
        ILogger<UpdateHallCommandHandler> logger,
        ICacheService cacheService)
    {
        _hallRepository = hallRepository;
        _cinemaRepository = cinemaRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(UpdateHallCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "UpdateHallCommand started. HallId: {HallId}, CinemaId: {CinemaId}, Name: {Name}, Capacity: {Capacity}",
            request.Id,
            request.Request.CinemaId,
            request.Request.Name,
            request.Request.Capacity);

        var hall = await _hallRepository.GetByIdAsync(request.Id, cancellationToken);

        if (hall is null)
        {
            _logger.LogWarning("Hall not found for update. Id: {HallId}", request.Id);
            return BaseResponse.Fail("Hall not found.");
        }

        var cinemaId = request.Request.CinemaId ?? hall.CinemaId;
        var hallName = request.Request.Name?.Trim() ?? hall.Name;

        var cinema = await _cinemaRepository.GetByIdAsync(cinemaId, cancellationToken);

        if (cinema is null)
        {
            _logger.LogWarning(
                "UpdateHallCommand failed. Cinema not found. CinemaId: {CinemaId}",
                cinemaId);

            return BaseResponse.Fail("Cinema not found.");
        }

        if (!cinema.IsActive)
        {
            _logger.LogWarning(
                "UpdateHallCommand failed. Cinema is inactive. CinemaId: {CinemaId}",
                cinemaId);

            return BaseResponse.Fail("Hall cannot be assigned to an inactive cinema.");
        }

        var duplicateExists = await _hallRepository.ExistsByNameInCinemaAsync(
            cinemaId,
            hallName,
            request.Id,
            cancellationToken);

        if (duplicateExists)
        {
            _logger.LogWarning(
                "UpdateHallCommand failed. Duplicate hall name in cinema. HallId: {HallId}, CinemaId: {CinemaId}, Name: {Name}",
                request.Id,
                cinemaId,
                hallName);

            return BaseResponse.Fail("A hall with the same name already exists in this cinema.");
        }

        _mapper.Map(request.Request, hall);

        if (request.Request.CinemaId.HasValue)
            hall.CinemaId = request.Request.CinemaId.Value;

        await _hallRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(HallCacheKey.ById(request.Id));
        await _cacheService.RemoveAsync(HallCacheKey.All);

        if (request.Request.CinemaId.HasValue)
        {
            await _cacheService.RemoveAsync(HallCacheKey.ByCinemaId(request.Request.CinemaId.Value));
        }
        else
        {
            await _cacheService.RemoveAsync(HallCacheKey.ByCinemaId(hall.CinemaId));
        }

        _logger.LogInformation(
            "UpdateHallCommand completed successfully. HallId: {HallId}",
            hall.Id);

        return BaseResponse.Ok("Hall updated successfully.");
    }
}