using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Tickets.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Tickets.Queries;

public sealed class GetTicketByIdQueryHandler
    : IRequestHandler<GetTicketByIdQuery, BaseResponse<GetTicketByIdResponse>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTicketByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetTicketByIdQueryHandler(
        ITicketRepository ticketRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetTicketByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<GetTicketByIdResponse>> Handle(
        GetTicketByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetTicketByIdQuery failed. Authenticated user not found.");
            return BaseResponse<GetTicketByIdResponse>.Fail("Authenticated user not found.");
        }

        var cacheKey = $"{TicketCacheKey.GetTicketByIdPrefix}{request.Id}:{userId}";

        var cached = await _cacheService.GetAsync<GetTicketByIdResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation(
                "GetTicketByIdQuery fetched from cache. TicketId: {TicketId}, UserId: {UserId}",
                request.Id,
                userId);

            return BaseResponse<GetTicketByIdResponse>.Ok(cached);
        }

        _logger.LogInformation(
            "GetTicketByIdQuery started. TicketId: {TicketId}, UserId: {UserId}",
            request.Id,
            userId);

        var ticket = await _ticketRepository.GetDetailedByIdAsync(request.Id, cancellationToken);

        if (ticket is null)
        {
            _logger.LogWarning(
                "GetTicketByIdQuery failed. Ticket not found. TicketId: {TicketId}",
                request.Id);

            return BaseResponse<GetTicketByIdResponse>.Fail("Ticket not found.");
        }

        if (ticket.UserId != userId)
        {
            _logger.LogWarning(
                "GetTicketByIdQuery failed. User does not own this ticket. TicketId: {TicketId}, OwnerUserId: {OwnerUserId}, CurrentUserId: {CurrentUserId}",
                ticket.Id,
                ticket.UserId,
                userId);

            return BaseResponse<GetTicketByIdResponse>.Fail("You are not allowed to access this ticket.");
        }

        var response = _mapper.Map<GetTicketByIdResponse>(ticket);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        _logger.LogInformation(
            "GetTicketByIdQuery completed successfully. TicketId: {TicketId}, UserId: {UserId}",
            ticket.Id,
            userId);

        return BaseResponse<GetTicketByIdResponse>.Ok(response);
    }
}