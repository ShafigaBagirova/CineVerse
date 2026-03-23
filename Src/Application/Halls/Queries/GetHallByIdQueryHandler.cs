using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Halls.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Halls.Queries;

public class GetHallByIdQueryHandler
    : IRequestHandler<GetHallByIdQuery, BaseResponse<GetHallByIdResponse>>
{
    private readonly IHallRepository _hallRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetHallByIdQueryHandler> _logger;

    public GetHallByIdQueryHandler(
        IHallRepository hallRepository,
        IMapper mapper,
        ILogger<GetHallByIdQueryHandler> logger)
    {
        _hallRepository = hallRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<GetHallByIdResponse>> Handle(
        GetHallByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetHallByIdQuery started. HallId: {HallId}",
            request.Id);

        var hall = await _hallRepository.GetByIdAsync(request.Id, cancellationToken);

        if (hall is null)
        {
            _logger.LogWarning(
                "GetHallByIdQuery failed. Hall not found. HallId: {HallId}",
                request.Id);

            return BaseResponse<GetHallByIdResponse>.Fail("Hall not found.");
        }

        if (!hall.IsActive)
        {
            _logger.LogWarning(
                "GetHallByIdQuery failed. Hall is inactive. HallId: {HallId}",
                request.Id);

            return BaseResponse<GetHallByIdResponse>.Fail("Hall not found.");
        }

        var response = _mapper.Map<GetHallByIdResponse>(hall);

        _logger.LogInformation(
            "GetHallByIdQuery completed successfully. HallId: {HallId}",
            request.Id);

        return BaseResponse<GetHallByIdResponse>.Ok(response, "Hall fetched successfully.");
    }
}