using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Screenings.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Screenings.Queries;

public class GetAllScreeningsQueryHandler
    : IRequestHandler<GetAllScreeningsQuery, BaseResponse<PaginatedResponse<GetAllScreeningsResponse>>>
{
    private readonly IScreeningRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllScreeningsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllScreeningsQueryHandler(
        IScreeningRepository repository,
        IMapper mapper,
        ILogger<GetAllScreeningsQueryHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllScreeningsResponse>>> Handle(
        GetAllScreeningsQuery request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;

        _logger.LogInformation(
            "GetAllScreeningsQuery started. PageNumber: {PageNumber}, PageSize: {PageSize}, MovieId: {MovieId}, HallId: {HallId}, Status: {Status}, Format: {Format}, IsActive: {IsActive}, DateFrom: {DateFrom}, DateTo: {DateTo}",
            dto.PageNumber,
            dto.PageSize,
            dto.MovieId,
            dto.HallId,
            dto.Status,
            dto.Format,
            dto.IsActive,
            dto.DateFrom,
            dto.DateTo);

        var cacheKey =
            $"screenings_all_movie_{dto.MovieId}_hall_{dto.HallId}_status_{dto.Status}_format_{dto.Format}_active_{dto.IsActive}_from_{dto.DateFrom:yyyyMMddHHmmss}_to_{dto.DateTo:yyyyMMddHHmmss}_page_{dto.PageNumber}_size_{dto.PageSize}";

        var cachedResponse =
            await _cacheService.GetAsync<PaginatedResponse<GetAllScreeningsResponse>>(cacheKey);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("GetAllScreeningsQuery response fetched from cache.");

            return BaseResponse<PaginatedResponse<GetAllScreeningsResponse>>
                .Ok(cachedResponse, "Screenings fetched from cache.");
        }

        var result = await _repository.GetPagedAsync(
            dto.PageNumber,
            dto.PageSize,
            dto.MovieId,
            dto.HallId,
            dto.Status,
            dto.Format,
            dto.IsActive,
            dto.DateFrom,
            dto.DateTo,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetAllScreeningsResponse>>(result.Items);

        var totalPages = (int)Math.Ceiling((double)result.TotalCount / dto.PageSize);

        var paginatedResponse = new PaginatedResponse<GetAllScreeningsResponse>
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
            "GetAllScreeningsQuery completed successfully. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            mappedItems.Count,
            result.TotalCount);

        return BaseResponse<PaginatedResponse<GetAllScreeningsResponse>>
            .Ok(paginatedResponse, "Screenings fetched successfully.");
    }
}