using Application.Cinemas.Dtos;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cinemas.Queries;

public class GetAllCinemasQueryHandler
    : IRequestHandler<GetAllCinemasQuery, BaseResponse<PaginatedResponse<GetAllCinemasResponse>>>
{
    private readonly ICinemaRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCinemasQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllCinemasQueryHandler(
        ICinemaRepository repository,
        IMapper mapper,
        ILogger<GetAllCinemasQueryHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllCinemasResponse>>> Handle(
        GetAllCinemasQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetAllCinemasQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.PageNumber,
            request.PageSize);

        var cacheKey = CinemaCacheKey.CinemasPaged(request.PageNumber, request.PageSize);

        var cachedResponse =
            await _cacheService.GetAsync<PaginatedResponse<GetAllCinemasResponse>>(cacheKey);

        if (cachedResponse is not null)
        {
            _logger.LogInformation(
                "Cinemas fetched from cache. PageNumber: {PageNumber}, PageSize: {PageSize}",
                request.PageNumber,
                request.PageSize);

            return BaseResponse<PaginatedResponse<GetAllCinemasResponse>>
                .Ok(cachedResponse, "Cinemas fetched from cache");
        }

        var (items, totalCount) = await _repository.GetPagedActiveAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetAllCinemasResponse>>(items);
        var paginatedResponse = new PaginatedResponse<GetAllCinemasResponse>
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        await _cacheService.SetAsync(cacheKey, paginatedResponse, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "Cinemas fetched from database and cached. Count: {Count}, TotalCount: {TotalCount}",
            mappedItems.Count,
            totalCount);

        return BaseResponse<PaginatedResponse<GetAllCinemasResponse>>
            .Ok(paginatedResponse, "Cinemas fetched successfully");
    }
}