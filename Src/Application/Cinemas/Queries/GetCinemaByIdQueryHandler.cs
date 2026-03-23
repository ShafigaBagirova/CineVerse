using Application.Cinemas.Dtos;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cinemas.Queries;

public class GetCinemaByIdQueryHandler
    : IRequestHandler<GetCinemaByIdQuery, BaseResponse<GetCinemaByIdResponse>>
{
    private readonly ICinemaRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCinemaByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetCinemaByIdQueryHandler(
        ICinemaRepository repository,
        IMapper mapper,
        ILogger<GetCinemaByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetCinemaByIdResponse>> Handle(
        GetCinemaByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetCinemaByIdQuery started for Id: {CinemaId}", request.Id);

        var cacheKey = CinemaCacheKey.CinemaById(request.Id);

        var cachedCinema = await _cacheService.GetAsync<GetCinemaByIdResponse>(cacheKey);

        if (cachedCinema is not null)
        {
            _logger.LogInformation("Cinema fetched from cache for Id: {CinemaId}", request.Id);
            return BaseResponse<GetCinemaByIdResponse>.Ok(cachedCinema, "Cinema fetched from cache");
        }

        var cinema = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (cinema is null)
        {
            _logger.LogWarning("Cinema not found for Id: {CinemaId}", request.Id);
            return BaseResponse<GetCinemaByIdResponse>.Fail("Cinema not found.");
        }

        if (!cinema.IsActive)
        {
            _logger.LogWarning("Inactive cinema requested. Id: {CinemaId}", request.Id);
            return BaseResponse<GetCinemaByIdResponse>.Fail("Cinema not found.");
        }

        var response = _mapper.Map<GetCinemaByIdResponse>(cinema);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation("Cinema fetched successfully from database for Id: {CinemaId}", request.Id);

        return BaseResponse<GetCinemaByIdResponse>.Ok(response, "Cinema fetched successfully");
    }
}