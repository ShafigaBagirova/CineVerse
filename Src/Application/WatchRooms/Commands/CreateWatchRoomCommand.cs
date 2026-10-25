using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;

namespace Application.WatchRooms.Commands;

public record CreateWatchRoomCommand(CreateWatchRoomRequest Request) : IRequest<BaseResponse<GetWatchRoomByIdResponse>>;
