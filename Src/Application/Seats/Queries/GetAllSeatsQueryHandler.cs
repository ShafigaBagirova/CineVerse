using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Seats.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace Application.Seats.Queries;

public sealed class GetAllSeatsQueryHandler
    : IRequestHandler<GetAllSeatsQuery, BaseResponse<PaginatedResponse<GetAllSeatsResponse>>>
{
    private readonly ISeatRepository _seatRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllSeatsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllSeatsQueryHandler(
        ISeatRepository seatRepository,
        IMapper mapper,
        ILogger<GetAllSeatsQueryHandler> logger,
        ICacheService cacheService)
    {
        _seatRepository = seatRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllSeatsResponse>>> Handle(
        GetAllSeatsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey =
            $"seats_all_hall_{request.Request.HallId}_row_{request.Request.Row}_type_{request.Request.Type}_active_{request.Request.IsActive}_page_{request.Request.PageNumber}_size_{request.Request.PageSize}";

        _logger.LogInformation(
            "GetAllSeatsQuery started. HallId: {HallId}, Row: {Row}, Type: {Type}, IsActive: {IsActive}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.Request.HallId,
            request.Request.Row,
            request.Request.Type,
            request.Request.IsActive,
            request.Request.PageNumber,
            request.Request.PageSize);

        var cached = await _cacheService.GetAsync<PaginatedResponse<GetAllSeatsResponse>>(cacheKey);
        if (cached is not null)
        {
            _logger.LogInformation("GetAllSeatsQuery served from cache.");
            return BaseResponse<PaginatedResponse<GetAllSeatsResponse>>.Ok(cached);
        }

        var query = _seatRepository.GetAll();

        if (request.Request.HallId.HasValue)
            query = query.Where(x => x.HallId == request.Request.HallId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Row))
        {
            var normalizedRow = request.Request.Row.Trim().ToUpperInvariant();
            query = query.Where(x => x.Row == normalizedRow);
        }

        if (request.Request.Type.HasValue)
            query = query.Where(x => x.Type == request.Request.Type.Value);

        if (request.Request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.Request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Row)
            .ThenBy(x => x.Number)
            .Skip((request.Request.PageNumber - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ProjectTo<GetAllSeatsResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var response = new PaginatedResponse<GetAllSeatsResponse>
        {
            Items = items,
            PageNumber = request.Request.PageNumber,
            PageSize = request.Request.PageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetAllSeatsQuery completed successfully. TotalCount: {TotalCount}",
            totalCount);

        return BaseResponse<PaginatedResponse<GetAllSeatsResponse>>.Ok(response);
    }
}