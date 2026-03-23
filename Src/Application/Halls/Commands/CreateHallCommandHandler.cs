using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Halls.Commands;

public class CreateHallCommandHandler : IRequestHandler<CreateHallCommand, BaseResponse>
{
    private readonly IHallRepository _hallRepository;
    private readonly ICinemaRepository _cinemaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateHallCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateHallCommandHandler(
        IHallRepository hallRepository,
        ICinemaRepository cinemaRepository,
        IMapper mapper,
        ILogger<CreateHallCommandHandler> logger,
        ICacheService cacheService)
    {
        _hallRepository = hallRepository;
        _cinemaRepository = cinemaRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(CreateHallCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "CreateHallCommand started. CinemaId: {CinemaId}, Name: {Name}, Capacity: {Capacity}",
            request.Request.CinemaId,
            request.Request.Name,
            request.Request.Capacity);

        var dto = request.Request;

        var cinema = await _cinemaRepository.GetByIdAsync(dto.CinemaId, cancellationToken);

        if (cinema is null)
        {
            _logger.LogWarning(
                "CreateHallCommand failed. Cinema not found. CinemaId: {CinemaId}",
                dto.CinemaId);

            return BaseResponse.Fail("Cinema not found.");
        }

        if (!cinema.IsActive)
        {
            _logger.LogWarning(
                "CreateHallCommand failed. Cinema is inactive. CinemaId: {CinemaId}",
                dto.CinemaId);

            return BaseResponse.Fail("Cinema is inactive.");
        }

        var hallExists = await _hallRepository.ExistsByNameInCinemaAsync(
            dto.CinemaId,
            dto.Name,
            cancellationToken);

        if (hallExists)
        {
            _logger.LogWarning(
                "CreateHallCommand failed. Duplicate hall name in cinema. CinemaId: {CinemaId}, Name: {Name}",
                dto.CinemaId,
                dto.Name);

            return BaseResponse.Fail(" Duplicate hall name in cinema.");
        }

        var hall = _mapper.Map<Hall>(dto);

        await _hallRepository.AddAsync(hall, cancellationToken);
        await _hallRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(HallCacheKey.All);
        await _cacheService.RemoveAsync(HallCacheKey.ByCinemaId(hall.CinemaId));
        _logger.LogInformation(
            "CreateHallCommand completed successfully. HallId: {HallId}, CinemaId: {CinemaId}",
            hall.Id,
            hall.CinemaId);

        return BaseResponse.Ok("Hall created successfully.");
    }
}