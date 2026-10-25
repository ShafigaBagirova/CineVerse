using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchRooms.Queries;

public class GetAllWatchRoomsQueryHandler
    : IRequestHandler<GetAllWatchRoomsQuery, BaseResponse<PaginatedResponse<GetAllWatchRoomsResponse>>>
{
    private readonly IWatchRoomRepository _watchRoomRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllWatchRoomsQueryHandler> _logger;

    public GetAllWatchRoomsQueryHandler(
        IWatchRoomRepository watchRoomRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetAllWatchRoomsQueryHandler> logger)
    {
        _watchRoomRepository = watchRoomRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<GetAllWatchRoomsResponse>>> Handle(
        GetAllWatchRoomsQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        _logger.LogInformation(
            "GetAllWatchRoomsQuery started. UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}, Search: {Search}, SortBy: {SortBy}, Desc: {Desc}",
            currentUserId,
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.SortBy,
            request.Desc);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<PaginatedResponse<GetAllWatchRoomsResponse>>.Fail("Authenticated user not found.");

        var result = await _watchRoomRepository.GetPagedByUserAsync(
            currentUserId,
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.SortBy,
            request.Desc,
            cancellationToken);

        var mappedItems = _mapper.Map<List<GetAllWatchRoomsResponse>>(result.Items);

        var totalPages = (int)Math.Ceiling((double)result.TotalCount / request.PageSize);

        var paginatedResponse = new PaginatedResponse<GetAllWatchRoomsResponse>
        {
            Items = mappedItems,
            TotalCount = result.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };

        _logger.LogInformation(
            "GetAllWatchRoomsQuery completed successfully. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
            mappedItems.Count,
            result.TotalCount);

        return BaseResponse<PaginatedResponse<GetAllWatchRoomsResponse>>
            .Ok(paginatedResponse, "Watch rooms fetched successfully.");
    }
}
