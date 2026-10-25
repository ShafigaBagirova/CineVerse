using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;

namespace Application.WatchRooms.Commands;

public record UpdateWatchRoomCommand(int Id, UpdateWatchRoomRequest Request) : IRequest<BaseResponse>;
