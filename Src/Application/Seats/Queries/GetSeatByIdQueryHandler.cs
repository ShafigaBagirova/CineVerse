using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Seats.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Seats.Queries;

public sealed class GetSeatByIdQueryHandler
    : IRequestHandler<GetSeatByIdQuery, BaseResponse<GetSeatByIdResponse>>
{
    private readonly ISeatRepository _seatRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSeatByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetSeatByIdQueryHandler(
        ISeatRepository seatRepository,
        IMapper mapper,
        ILogger<GetSeatByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _seatRepository = seatRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetSeatByIdResponse>> Handle(
        GetSeatByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"seat_{request.Id}";

        _logger.LogInformation("GetSeatByIdQuery started. SeatId: {SeatId}", request.Id);

        var cached = await _cacheService.GetAsync<GetSeatByIdResponse>(cacheKey);
        if (cached is not null)
        {
            _logger.LogInformation("Seat fetched from cache. SeatId: {SeatId}", request.Id);
            return BaseResponse<GetSeatByIdResponse>.Ok(cached);
        }

        var seat = await _seatRepository.GetByIdAsync(request.Id, cancellationToken);

        if (seat is null)
        {
            _logger.LogWarning("GetSeatByIdQuery failed. Seat not found. SeatId: {SeatId}", request.Id);
            return BaseResponse<GetSeatByIdResponse>.Fail("Seat not found.");
        }

        var response = _mapper.Map<GetSeatByIdResponse>(seat);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation("Seat fetched from database and cached. SeatId: {SeatId}", request.Id);

        return BaseResponse<GetSeatByIdResponse>.Ok(response);
    }
}