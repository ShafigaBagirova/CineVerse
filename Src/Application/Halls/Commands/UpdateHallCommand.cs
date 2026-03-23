using Application.Common.Responses;
using Application.Halls.Dtos;
using MediatR;

namespace Application.Halls.Commands;

public record UpdateHallCommand(int Id, UpdateHallRequest Request) : IRequest<BaseResponse>;
