using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;

namespace Application.WatchRooms.Commands;

public record JoinWatchRoomCommand(string Code) : IRequest<BaseResponse<GetWatchRoomByIdResponse>>;
