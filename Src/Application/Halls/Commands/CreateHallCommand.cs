using Application.Cinemas.Dtos;
using Application.Common.Responses;
using Application.Halls.Dtos;
using MediatR;

namespace Application.Halls.Commands;

public record CreateHallCommand(CreateHallRequest Request) : IRequest<BaseResponse>;
