using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;

namespace Application.WatchRooms.Queries;

public record GetWatchRoomByIdQuery(int Id) : IRequest<BaseResponse<GetWatchRoomByIdResponse>>;
