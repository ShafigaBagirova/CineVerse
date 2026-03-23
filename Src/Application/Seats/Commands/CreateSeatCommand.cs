using Application.Common.Responses;
using Application.Seats.Dtos;
using MediatR;

namespace Application.Seats.Commands;

public sealed record CreateSeatCommand(CreateSeatRequest Request) : IRequest<BaseResponse>;
