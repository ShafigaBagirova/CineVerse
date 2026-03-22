using Application.Cinemas.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cinemas.Queries;

public class GetCinemasByLocationQueryHandler
    : IRequestHandler<GetCinemasByLocationQuery, BaseResponse<List<GetAllCinemasResponse>>>
{
    private readonly ICinemaRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCinemasByLocationQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetCinemasByLocationQueryHandler(
        ICinemaRepository repository,
        IMapper mapper,
        ILogger<GetCinemasByLocationQueryHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<GetAllCinemasResponse>>> Handle(
        GetCinemasByLocationQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetCinemasByLocationQuery started. Country: {Country}, City: {City}",
            request.Country,
            request.City);

        var normalizedCountry = request.Country?.Trim().ToLower() ?? string.Empty;
        var normalizedCity = request.City?.Trim().ToLower() ?? string.Empty;

        var cacheKey = $"cinemas:location:country:{normalizedCountry}:city:{normalizedCity}";

        var cachedResponse = await _cacheService.GetAsync<List<GetAllCinemasResponse>>(cacheKey);

        if (cachedResponse is not null)
        {
            _logger.LogInformation(
                "Cinemas fetched from cache by location. Country: {Country}, City: {City}, Count: {Count}",
                request.Country,
                request.City,
                cachedResponse.Count);

            return BaseResponse<List<GetAllCinemasResponse>>.Ok(cachedResponse, "Cinemas fetched from cache");
        }

        var cinemas = await _repository.GetByLocationAsync(
            request.Country,
            request.City,
            cancellationToken);

        var response = _mapper.Map<List<GetAllCinemasResponse>>(cinemas);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "Cinemas fetched successfully by location. Country: {Country}, City: {City}, Count: {Count}",
            request.Country,
            request.City,
            response.Count);

        return BaseResponse<List<GetAllCinemasResponse>>.Ok(response, "Cinemas fetched successfully");
    }
}