using Application.Common.Responses;
using Application.Seats.Dtos;
using MediatR;

namespace Application.Seats.Commands;

public sealed record UpdateSeatCommand(int Id, UpdateSeatRequest Request) : IRequest<BaseResponse>;