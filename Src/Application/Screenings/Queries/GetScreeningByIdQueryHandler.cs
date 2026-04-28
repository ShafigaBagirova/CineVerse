using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Screenings.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Screenings.Queries;

public sealed class GetScreeningByIdQueryHandler
    : IRequestHandler<GetScreeningByIdQuery, BaseResponse<GetScreeningByIdResponse>>
{
    private readonly IScreeningRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetScreeningByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetScreeningByIdQueryHandler(
        IScreeningRepository repository,
        IMapper mapper,
        ILogger<GetScreeningByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetScreeningByIdResponse>> Handle(
        GetScreeningByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"screening_{request.Id}";
        _logger.LogInformation("Screening-by-id cache key used: {CacheKey}", cacheKey);

        _logger.LogInformation(
            "GetScreeningByIdQuery started. ScreeningId: {ScreeningId}",
            request.Id);

        var cached = await _cacheService.GetAsync<GetScreeningByIdResponse>(cacheKey);

        if (cached is not null)
        {
            _logger.LogInformation(
                "GetScreeningByIdQuery served from cache. ScreeningId: {ScreeningId}",
                request.Id);

            return BaseResponse<GetScreeningByIdResponse>.Ok(cached);
        }

        var screening = await _repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (screening is null)
        {
            _logger.LogWarning(
                "GetScreeningByIdQuery failed. Screening not found. ScreeningId: {ScreeningId}",
                request.Id);

            return BaseResponse<GetScreeningByIdResponse>.Fail("Screening not found.");
        }

        var response = _mapper.Map<GetScreeningByIdResponse>(screening);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Screening-by-id cache set: {CacheKey}", cacheKey);

        _logger.LogInformation(
            "GetScreeningByIdQuery completed successfully. ScreeningId: {ScreeningId}",
            request.Id);

        return BaseResponse<GetScreeningByIdResponse>.Ok(response);
    }
}