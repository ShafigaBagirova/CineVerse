using Application.Common.Responses;
using MediatR;

namespace Application.WatchRooms.Commands;

public record DeleteWatchRoomCommand(int Id) : IRequest<BaseResponse>;
