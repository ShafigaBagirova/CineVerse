using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.SeatHolds.Queries;

public sealed class GetSeatHoldByIdQueryHandler
    : IRequestHandler<GetSeatHoldByIdQuery, BaseResponse<GetSeatHoldByIdResponse>>
{
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSeatHoldByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetSeatHoldByIdQueryHandler(
        ISeatHoldRepository seatHoldRepository,
        IMapper mapper,
        ILogger<GetSeatHoldByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _seatHoldRepository = seatHoldRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetSeatHoldByIdResponse>> Handle(
        GetSeatHoldByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{SeatHoldCacheKeys.GetSeatHoldByIdPrefix}{request.Id}";

        var cached = await _cacheService.GetAsync<GetSeatHoldByIdResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation(
                "SeatHold fetched from cache. SeatHoldId: {SeatHoldId}",
                request.Id);

            return BaseResponse<GetSeatHoldByIdResponse>.Ok(cached);
        }

        _logger.LogInformation(
            "GetSeatHoldByIdQuery started. SeatHoldId: {SeatHoldId}",
            request.Id);

        var seatHold = await _seatHoldRepository.GetByIdAsync(request.Id, cancellationToken);

        if (seatHold is null)
        {
            _logger.LogWarning(
                "GetSeatHoldByIdQuery failed. Seat hold not found. SeatHoldId: {SeatHoldId}",
                request.Id);

            return BaseResponse<GetSeatHoldByIdResponse>.Fail("Seat hold not found.");
        }

        var response = _mapper.Map<GetSeatHoldByIdResponse>(seatHold);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        _logger.LogInformation(
            "SeatHold fetched successfully. SeatHoldId: {SeatHoldId}",
            seatHold.Id);

        return BaseResponse<GetSeatHoldByIdResponse>.Ok(response);
    }
}
