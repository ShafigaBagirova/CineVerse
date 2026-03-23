using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Halls.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Halls.Queries;

public class GetAllHallsQueryHandler
    : IRequestHandler<GetAllHallsQuery, BaseResponse<PaginatedResponse<GetAllHallsResponse>>>
{
    private readonly IHallRepository _hallRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllHallsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllHallsQueryHandler(
        IHallRepository hallRepository,
        IMapper mapper,
        ILogger<GetAllHallsQueryHandler> logger,
        ICacheService cacheService)
    {
        _hallRepository = hallRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllHallsResponse>>> Handle(
        GetAllHallsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetAllHallsQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}, CinemaId: {CinemaId}, Search: {Search}, SortBy: {SortBy}, Desc: {Desc}",
            request.PageNumber,
            request.PageSize,
            request.CinemaId,
            request.Search,
            request.SortBy,
            request.Desc);

        var cacheKey = HallCacheKey.HallsPaged(
            request.PageNumber,
            request.PageSize,
            request.CinemaId,
            request.Search,
            request.SortBy,
            request.Desc);

        var cachedResponse =
            await _cacheService.GetAsync<PaginatedResponse<GetAllHallsResponse>>(cacheKey);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("GetAllHallsQuery response fetched from cache.");

            return BaseResponse<PaginatedResponse<GetAllHallsResponse>>
                .Ok(cachedResponse, "Halls fetched from cache.");
        }

        var result = await _hallRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.CinemaId,
            request.Search,
            request.SortBy,
            request.Desc,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetAllHallsResponse>>(result.Items);

        var totalPages = (int)Math.Ceiling((double)result.TotalCount / request.PageSize);

        var paginatedResponse = new PaginatedResponse<GetAllHallsResponse>
        {
            Items = mappedItems,
            TotalCount = result.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };

        await _cacheService.SetAsync(cacheKey, paginatedResponse, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetAllHallsQuery completed successfully. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            mappedItems.Count,
            result.TotalCount);

        return BaseResponse<PaginatedResponse<GetAllHallsResponse>>
            .Ok(paginatedResponse, "Halls fetched successfully.");
    }
}