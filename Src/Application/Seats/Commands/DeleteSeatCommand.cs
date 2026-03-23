using Application.Common.Responses;
using MediatR;

namespace Application.Seats.Commands;


public sealed record DeleteSeatCommand(int Id) : IRequest<BaseResponse>;