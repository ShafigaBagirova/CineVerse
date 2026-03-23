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
        var dto = request.Request;

        _logger.LogInformation(
            "GetAllCinemasQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}, Country: {Country}, City: {City}, Search: {Search}, SortBy: {SortBy}, Desc: {Desc}",
            dto.PageNumber,
            dto.PageSize,
            dto.Country,
            dto.City,
            dto.Search,
            dto.SortBy,
            dto.Desc);

        var cacheKey = CinemaCacheKey.CinemasPaged(
            dto.PageNumber,
            dto.PageSize,
            dto.Country,
            dto.City,
            dto.Search,
            dto.SortBy,
            dto.Desc);

        var cachedResponse =
            await _cacheService.GetAsync<PaginatedResponse<GetAllCinemasResponse>>(cacheKey);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("GetAllCinemasQuery response fetched from cache.");

            return BaseResponse<PaginatedResponse<GetAllCinemasResponse>>
                .Ok(cachedResponse, "Cinemas fetched from cache");
        }

        var result = await _repository.GetPagedActiveAsync(
            dto.PageNumber,
            dto.PageSize,
            dto.Country,
            dto.City,
            dto.Search,
            dto.SortBy,
            dto.Desc,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetAllCinemasResponse>>(result.Items);

        var totalPages = (int)Math.Ceiling((double)result.TotalCount / dto.PageSize);

        var paginatedResponse = new PaginatedResponse<GetAllCinemasResponse>
        {
            Items = mappedItems,
            TotalCount = result.TotalCount,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = dto.PageNumber > 1,
            HasNextPage = dto.PageNumber < totalPages
        };

        await _cacheService.SetAsync(cacheKey, paginatedResponse, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetAllCinemasQuery completed successfully. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            mappedItems.Count,
            result.TotalCount);

        return BaseResponse<PaginatedResponse<GetAllCinemasResponse>>
            .Ok(paginatedResponse, "Cinemas fetched successfully");
    }
}